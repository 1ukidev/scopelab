using System;
using System.Collections.Generic;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Textures;
using osuTK;
using osuTK.Graphics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using RectangleF = osu.Framework.Graphics.Primitives.RectangleF;

namespace ScopeLab.Graphics.Display;

/// <summary>
/// Simulates a phosphor screen excited by an electron beam.
/// </summary>
/// <remarks>
/// Traces are rendered as dense chains of gaussian splats whose energy is spread according to the beam speed,
/// so slow portions glow brightly while fast edges fade out. A persistent frame buffer accumulates past sweeps
/// and decays over time to produce the characteristic afterglow.
/// </remarks>
public partial class PhosphorLayer : Drawable, ITexturedShaderDrawable
{
    private const int beam_texture_size = 64;

    /// <summary>
    /// Standard deviation of the core, relative to the half-size of the beam texture.
    /// </summary>
    private const float core_sigma = 0.1f;

    public IShader TextureShader { get; private set; } = null!;

    public readonly List<BeamTrace> Traces = new List<BeamTrace>();

    /// <summary>
    /// Standard deviation of the beam's core, in local units.
    /// </summary>
    public float BeamSigma { get; set; } = 0.75f;

    /// <summary>
    /// Afterglow time constant in seconds. <see cref="double.PositiveInfinity"/> never fades.
    /// </summary>
    public double Persistence { get; set; } = 0.05;

    /// <summary>
    /// Brightness of the afterglow relative to the live beam.
    /// </summary>
    public float AfterglowStrength { get; set; } = 0.45f;

    private Texture beamTexture = null!;
    private readonly PersistenceBuffer persistenceBuffer = new PersistenceBuffer();
    private int clearVersion;

    [BackgroundDependencyLoader]
    private void load(ShaderManager shaders, IRenderer renderer)
    {
        TextureShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, FragmentShaderDescriptor.TEXTURE);
        beamTexture = createBeamTexture(renderer);
    }

    /// <summary>
    /// Erases the afterglow.
    /// </summary>
    public void ClearAfterglow()
    {
        clearVersion++;
        Invalidate(Invalidation.DrawNode);
    }

    /// <summary>
    /// Must be called after mutating <see cref="Traces"/>.
    /// </summary>
    public void Refresh() => Invalidate(Invalidation.DrawNode);

    protected override DrawNode CreateDrawNode() => new PhosphorDrawNode(this);

    protected override void Dispose(bool isDisposing)
    {
        base.Dispose(isDisposing);

        persistenceBuffer.Dispose();
        beamTexture?.Dispose();
    }

    /// <summary>
    /// The beam profile: a sharp core surrounded by a faint halo. Colour is premultiplied so the texture works with max-blending.
    /// </summary>
    private static Texture createBeamTexture(IRenderer renderer)
    {
        var image = new Image<Rgba32>(beam_texture_size, beam_texture_size);

        for (int y = 0; y < beam_texture_size; y++)
        {
            for (int x = 0; x < beam_texture_size; x++)
            {
                float dx = (x + 0.5f) / beam_texture_size * 2 - 1;
                float dy = (y + 0.5f) / beam_texture_size * 2 - 1;
                float r2 = dx * dx + dy * dy;

                float value = MathF.Exp(-r2 / (2 * core_sigma * core_sigma)) + 0.12f * MathF.Exp(-r2 / (2 * 0.33f * 0.33f));
                value *= Math.Clamp((1 - MathF.Sqrt(r2)) / 0.2f, 0, 1);

                byte b = (byte)Math.Clamp(MathF.Round(value * 255), 0, 255);
                image[x, y] = new Rgba32(b, b, b, b);
            }
        }

        var texture = renderer.CreateTexture(beam_texture_size, beam_texture_size);
        texture.SetData(new TextureUpload(image));
        return texture;
    }

    private class PersistenceBuffer : IDisposable
    {
        public IFrameBuffer? FrameBuffer;
        public Vector2 Size;
        public int ClearVersion = -1;
        public readonly Stopwatch Clock = Stopwatch.StartNew();
        public double LastUpdate;
        public double PendingDecayTime;

        public void Dispose() => FrameBuffer?.Dispose();
    }

    private class TraceSnapshot
    {
        public Vector2[] Points = new Vector2[256];
        public int Count;
        public Color4 Colour;
        public float Intensity;
        public float ReferenceSpacing;
        public bool ShowBeamSpot;
    }

    private readonly struct Splat(Vector2 position, Color4 colour, float brightness, float scale)
    {
        public readonly Vector2 Position = position;
        public readonly Color4 Colour = colour;
        public readonly float Brightness = brightness;
        public readonly float Scale = scale;
    }

    private class PhosphorDrawNode(PhosphorLayer source) : TexturedShaderDrawNode(source)
    {
        private const int splat_budget = 40000;

        /// <summary>
        /// Fraction of the nominal brightness that remains visible no matter how fast the beam moves.
        /// </summary>
        private const float minimum_brightness = 0.2f;

        private static readonly BlendingParameters additive_premultiplied = new BlendingParameters
        {
            Source = BlendingType.One,
            Destination = BlendingType.One,
            SourceAlpha = BlendingType.Zero,
            DestinationAlpha = BlendingType.One,
            RGBEquation = BlendingEquation.Add,
            AlphaEquation = BlendingEquation.Add,
        };

        private static readonly BlendingParameters maximum = additive_premultiplied with { RGBEquation = BlendingEquation.Max };

        private static readonly BlendingParameters multiply = additive_premultiplied with
        {
            Source = BlendingType.Zero,
            Destination = BlendingType.SrcColor,
        };

        private static readonly BlendingParameters subtract = additive_premultiplied with { RGBEquation = BlendingEquation.ReverseSubtract };

        protected new PhosphorLayer Source => (PhosphorLayer)base.Source;

        private readonly List<TraceSnapshot> traces = new List<TraceSnapshot>();
        private readonly List<Splat> splats = new List<Splat>();

        private int traceCount;
        private Texture texture = null!;
        private PersistenceBuffer buffer = null!;
        private RectangleF screenSpaceRectangle;
        private Quad screenSpaceQuad;
        private float beamSigma;
        private double persistence;
        private float afterglowStrength;
        private float alpha;
        private int clearVersion;

        public override void ApplyState()
        {
            base.ApplyState();

            texture = Source.beamTexture;
            buffer = Source.persistenceBuffer;
            screenSpaceQuad = Source.ScreenSpaceDrawQuad;
            screenSpaceRectangle = screenSpaceQuad.AABBFloat;
            beamSigma = Source.BeamSigma;
            persistence = Source.Persistence;
            afterglowStrength = Source.AfterglowStrength;
            alpha = DrawColourInfo.Colour.MaxAlpha;
            clearVersion = Source.clearVersion;

            traceCount = Source.Traces.Count;

            while (traces.Count < traceCount)
                traces.Add(new TraceSnapshot());

            for (int i = 0; i < traceCount; i++)
            {
                var trace = Source.Traces[i];
                var snapshot = traces[i];

                if (snapshot.Points.Length < trace.Points.Count)
                    snapshot.Points = new Vector2[trace.Points.Count * 2];

                trace.Points.CopyTo(snapshot.Points);
                snapshot.Count = trace.Points.Count;
                snapshot.Colour = trace.Colour;
                snapshot.Intensity = trace.Intensity;
                snapshot.ReferenceSpacing = trace.ReferenceSpacing;
                snapshot.ShowBeamSpot = trace.ShowBeamSpot;
            }
        }

        protected override void Draw(IRenderer renderer)
        {
            base.Draw(renderer);

            if (screenSpaceRectangle.Width < 1 || screenSpaceRectangle.Height < 1)
                return;

            generateSplats();

            BindTextureShader(renderer);

            updateAfterglow(renderer);

            renderer.SetBlend(additive_premultiplied);
            renderer.DrawFrameBuffer(buffer.FrameBuffer!, screenSpaceQuad, new Color4(afterglowStrength * alpha, afterglowStrength * alpha, afterglowStrength * alpha, 1));

            foreach (var splat in splats)
                drawSplat(renderer, splat, splat.Colour);

            UnbindTextureShader(renderer);
        }

        private void generateSplats()
        {
            splats.Clear();

            float scale = Vector2Extensions.Transform(Vector2.UnitX, DrawInfo.Matrix).X - Vector2Extensions.Transform(Vector2.Zero, DrawInfo.Matrix).X;
            float sigma = beamSigma * scale;
            float spacing = sigma * 1.25f;
            float halfSize = sigma / core_sigma;

            // Integral of a gaussian cross-section; converts a linear energy density into per-splat brightness.
            float lineIntegral = MathF.Sqrt(MathF.Tau) * sigma;

            var cullRectangle = screenSpaceRectangle.Inflate(halfSize);

            float totalLength = 0;
            int totalSegments = 0;

            for (int t = 0; t < traceCount; t++)
            {
                var trace = traces[t];

                for (int i = 0; i < trace.Count; i++)
                    trace.Points[i] = Vector2Extensions.Transform(trace.Points[i], DrawInfo.Matrix);

                for (int i = 1; i < trace.Count; i++)
                    totalLength += Vector2.Distance(trace.Points[i - 1], trace.Points[i]);

                totalSegments += trace.Count;
            }

            spacing = Math.Max(spacing, totalLength / Math.Max(1, splat_budget - totalSegments));

            for (int t = 0; t < traceCount; t++)
            {
                var trace = traces[t];

                if (trace.Intensity <= 0 || trace.Count < 2)
                    continue;

                float referenceSpacing = trace.ReferenceSpacing * scale;
                var colour = trace.Colour;

                for (int i = 1; i < trace.Count; i++)
                {
                    Vector2 from = trace.Points[i - 1];
                    Vector2 to = trace.Points[i];

                    if (!segmentVisible(cullRectangle, from, to))
                        continue;

                    float length = Vector2.Distance(from, to);
                    int count = Math.Max(1, (int)MathF.Ceiling(length / spacing));

                    float lineBrightness = trace.Intensity * Math.Max(referenceSpacing / Math.Max(length, 0.01f), minimum_brightness);
                    float splatBrightness = lineBrightness * (length / count) / lineIntegral;

                    for (int k = 0; k < count; k++)
                    {
                        var position = Vector2.Lerp(from, to, (k + 0.5f) / count);
                        splats.Add(new Splat(position, premultiply(colour, splatBrightness * alpha), Math.Min(1, lineBrightness) * alpha, halfSize));
                    }
                }

                if (trace.ShowBeamSpot)
                {
                    var spotColour = premultiply(new Color4((colour.R + 1) / 2, (colour.G + 1) / 2, (colour.B + 1) / 2, 1), alpha);
                    splats.Add(new Splat(trace.Points[trace.Count - 1], spotColour, alpha, halfSize * 1.6f));
                }
            }
        }

        private void updateAfterglow(IRenderer renderer)
        {
            buffer.FrameBuffer ??= renderer.CreateFrameBuffer();

            double nowSeconds = buffer.Clock.Elapsed.TotalSeconds;
            double elapsed = Math.Clamp(nowSeconds - buffer.LastUpdate, 0, 1);
            buffer.LastUpdate = nowSeconds;

            var size = new Vector2(MathF.Ceiling(screenSpaceRectangle.Width), MathF.Ceiling(screenSpaceRectangle.Height));
            bool requiresClear = buffer.Size != size || buffer.ClearVersion != clearVersion;

            buffer.Size = size;
            buffer.ClearVersion = clearVersion;
            buffer.FrameBuffer.Size = size;

            var viewport = new RectangleI(0, 0, (int)size.X, (int)size.Y);

            renderer.PushMaskingInfo(new MaskingInfo
            {
                ScreenSpaceAABB = new RectangleI((int)MathF.Floor(screenSpaceRectangle.X), (int)MathF.Floor(screenSpaceRectangle.Y), viewport.Width + 1, viewport.Height + 1),
                MaskingRect = screenSpaceRectangle,
                ToMaskingSpace = Matrix3.Identity,
                BlendRange = 1,
                AlphaExponent = 1,
            }, true);

            renderer.PushViewport(viewport);
            renderer.PushScissor(viewport);
            renderer.PushScissorOffset(new Vector2I((int)MathF.Floor(screenSpaceRectangle.X), (int)MathF.Floor(screenSpaceRectangle.Y)));

            buffer.FrameBuffer.Bind();
            renderer.PushOrtho(screenSpaceRectangle);

            if (requiresClear)
                renderer.Clear(new ClearInfo(Color4.Black));

            decay(renderer, elapsed);

            renderer.SetBlend(maximum);

            foreach (var splat in splats)
            {
                float b = splat.Brightness;
                var c = splat.Colour;
                float peak = Math.Max(c.R, Math.Max(c.G, c.B));

                if (peak > 0)
                    drawSplat(renderer, splat, new Color4(c.R / peak * b, c.G / peak * b, c.B / peak * b, 1));
            }

            renderer.PopOrtho();
            buffer.FrameBuffer.Unbind();

            // The viewport must be restored first, as scissor rectangles are clipped against it.
            renderer.PopScissorOffset();
            renderer.PopViewport();
            renderer.PopScissor();
            renderer.PopMaskingInfo();
        }

        private void decay(IRenderer renderer, double elapsed)
        {
            if (double.IsPositiveInfinity(persistence))
                return;

            // An 8-bit buffer can't represent tiny per-frame decays, so they're batched until they become significant.
            buffer.PendingDecayTime += elapsed;

            double factor = Math.Exp(-buffer.PendingDecayTime / Math.Max(persistence, 0.04));

            if (factor > 0.94)
                return;

            buffer.PendingDecayTime = 0;

            renderer.SetBlend(multiply);
            renderer.DrawQuad(renderer.WhitePixel, screenSpaceRectangle, new Color4((float)factor, (float)factor, (float)factor, 1));

            const float residue = 1.5f / 255;

            renderer.SetBlend(subtract);
            renderer.DrawQuad(renderer.WhitePixel, screenSpaceRectangle, new Color4(residue, residue, residue, 1));
        }

        private void drawSplat(IRenderer renderer, in Splat splat, Color4 colour)
        {
            float h = splat.Scale;
            var p = splat.Position;
            renderer.DrawQuad(texture, new Quad(p.X - h, p.Y - h, h * 2, h * 2), colour);
        }

        private static Color4 premultiply(Color4 colour, float amount) => new Color4(colour.R * amount, colour.G * amount, colour.B * amount, 1);

        private static bool segmentVisible(RectangleF bounds, Vector2 a, Vector2 b) =>
            !(a.X < bounds.Left && b.X < bounds.Left)
            && !(a.X > bounds.Right && b.X > bounds.Right)
            && !(a.Y < bounds.Top && b.Y < bounds.Top)
            && !(a.Y > bounds.Bottom && b.Y > bounds.Bottom);
    }
}
