using System;
using System.IO;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Framework.Platform;
using ScopeLab.Simulation;

namespace ScopeLab.Configuration;

/// <summary>
/// Remembers the instrument settings between sessions.
/// </summary>
/// <remarks>
/// The settings are restored as soon as this component loads, so it should be loaded before anything that displays them.
/// Changes are written shortly after they happen, and once more when the component is disposed.
/// </remarks>
public partial class ScopeStateStore : Component
{
    public const string FILE_NAME = "scope-state.json";

    private const double save_interval = 1000;

    [Resolved]
    private Oscilloscope scope { get; set; } = null!;

    [Resolved]
    private Storage storage { get; set; } = null!;

    private string lastSaved = string.Empty;

    [BackgroundDependencyLoader]
    private void load()
    {
        restore();
        Scheduler.AddDelayed(save, save_interval, true);
    }

    private void restore()
    {
        if (!storage.Exists(FILE_NAME))
            return;

        try
        {
            using var reader = new StreamReader(storage.GetStream(FILE_NAME));
            string json = reader.ReadToEnd();

            if (ScopeState.TryParse(json, out var state))
            {
                state!.ApplyTo(scope);
                lastSaved = json;
            }
            else
                Logger.Log($"Ignoring unreadable {FILE_NAME}, starting with the factory setup.", level: LogLevel.Important);
        }
        catch (IOException e)
        {
            Logger.Error(e, $"Failed to read {FILE_NAME}.");
        }
    }

    private void save()
    {
        string json = ScopeState.Capture(scope).ToJson();

        if (json == lastSaved)
            return;

        try
        {
            using (var stream = storage.CreateFileSafely(FILE_NAME))
            using (var writer = new StreamWriter(stream))
                writer.Write(json);

            lastSaved = json;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            Logger.Error(e, $"Failed to save {FILE_NAME}.");
        }
    }

    protected override void Dispose(bool isDisposing)
    {
        if (IsLoaded)
            save();

        base.Dispose(isDisposing);
    }
}
