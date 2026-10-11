using osu.Framework.Localisation;

namespace ScopeLab.Localisation;

/// <summary>
/// Texts of the function generator's panel.
/// </summary>
public static class GeneratorStrings
{
    public static readonly StringTable TABLE = new StringTable("generator")
    {
        ["title"] = ("FUNCTION GENERATOR  ·  20 MHz  ·  DUAL OUTPUT", "GERADOR DE FUNÇÕES  ·  20 MHz  ·  SAÍDA DUPLA"),
        ["output"] = ("OUT {0}", "SAÍDA {0}"),
        ["output_tooltip"] = ("Enables output {0}, wired to {1} of the scope", "Liga a saída {0}, conectada ao {1} do osciloscópio"),
        ["waveform_tooltip"] = ("Waveform", "Forma de onda"),
        ["frequency"] = ("Freq", "Freq"),
        ["frequency_tooltip"] = ("Frequency. Hold Shift for fine tuning: try slightly detuned channels in XY mode!",
            "Frequência. Segure Shift para ajuste fino: experimente canais levemente dessintonizados no modo XY!"),
        ["amplitude"] = ("Ampl", "Ampl"),
        ["amplitude_tooltip"] = ("Peak-to-peak amplitude", "Amplitude pico a pico"),
        ["offset"] = ("Offset", "Offset"),
        ["offset_tooltip"] = ("DC offset added to the waveform", "Nível DC somado à forma de onda"),
        ["phase"] = ("Phase", "Fase"),
        ["phase_tooltip"] = ("Phase shift. Shapes the figures drawn in XY mode.", "Defasagem. Muda as figuras desenhadas no modo XY."),
        ["noise"] = ("Noise", "Ruído"),
        ["noise_tooltip"] = ("Adds random noise (RMS)", "Adiciona ruído aleatório (RMS)"),
    };

    public static LocalisableString Title => TABLE.Get("title");
    public static LocalisableString Output(int number) => TABLE.Get("output", number);
    public static LocalisableString OutputTooltip(int number, string channel) => TABLE.Get("output_tooltip", number, channel);
    public static LocalisableString WaveformTooltip => TABLE.Get("waveform_tooltip");
    public static LocalisableString Frequency => TABLE.Get("frequency");
    public static LocalisableString FrequencyTooltip => TABLE.Get("frequency_tooltip");
    public static LocalisableString Amplitude => TABLE.Get("amplitude");
    public static LocalisableString AmplitudeTooltip => TABLE.Get("amplitude_tooltip");
    public static LocalisableString Offset => TABLE.Get("offset");
    public static LocalisableString OffsetTooltip => TABLE.Get("offset_tooltip");
    public static LocalisableString Phase => TABLE.Get("phase");
    public static LocalisableString PhaseTooltip => TABLE.Get("phase_tooltip");
    public static LocalisableString Noise => TABLE.Get("noise");
    public static LocalisableString NoiseTooltip => TABLE.Get("noise_tooltip");
}
