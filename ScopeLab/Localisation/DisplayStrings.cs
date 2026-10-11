using osu.Framework.Localisation;

namespace ScopeLab.Localisation;

/// <summary>
/// Texts of the screen and the measurement panel.
/// </summary>
/// <remarks>
/// On-screen status readouts (RUN, TRIG'D, CH1, V/div...) stay untranslated, as on real instruments.
/// </remarks>
public static class DisplayStrings
{
    public static readonly StringTable TABLE = new StringTable("display")
    {
        ["channel_marker_tooltip"] = ("{0} ground level. Drag to move the trace, double-click to reset.",
            "Nível de terra do {0}. Arraste para mover o traço, duplo clique restaura."),
        ["trigger_level_tooltip"] = ("Trigger level. Drag to set the threshold the signal must cross.",
            "Nível de disparo. Arraste para definir o limiar que o sinal precisa cruzar."),
        ["trigger_position_tooltip"] = ("Trigger point. Drag to shift the waveform horizontally.",
            "Ponto de disparo. Arraste para deslocar a forma de onda na horizontal."),
        ["measure"] = ("Measure", "Medidas"),
        ["counter"] = ("Counter", "Contador"),
        ["frequency"] = ("Freq", "Freq"),
        ["period"] = ("Period", "Período"),
        ["mean"] = ("Mean", "Média"),
        ["duty_cycle"] = ("Duty", "Ciclo útil"),
        ["maximum"] = ("Max", "Máx"),
        ["minimum"] = ("Min", "Mín"),
    };

    public static LocalisableString ChannelMarkerTooltip(string channel) => TABLE.Get("channel_marker_tooltip", channel);
    public static LocalisableString TriggerLevelTooltip => TABLE.Get("trigger_level_tooltip");
    public static LocalisableString TriggerPositionTooltip => TABLE.Get("trigger_position_tooltip");
    public static LocalisableString Measure => TABLE.Get("measure");
    public static LocalisableString Counter => TABLE.Get("counter");
    public static LocalisableString Frequency => TABLE.Get("frequency");
    public static LocalisableString Period => TABLE.Get("period");
    public static LocalisableString Mean => TABLE.Get("mean");
    public static LocalisableString DutyCycle => TABLE.Get("duty_cycle");
    public static LocalisableString Maximum => TABLE.Get("maximum");
    public static LocalisableString Minimum => TABLE.Get("minimum");
}
