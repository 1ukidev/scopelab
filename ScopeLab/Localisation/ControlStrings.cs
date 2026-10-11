using osu.Framework.Localisation;

namespace ScopeLab.Localisation;

/// <summary>
/// Texts of the oscilloscope's control panel.
/// </summary>
public static class ControlStrings
{
    public static readonly StringTable TABLE = new StringTable("controls")
    {
        ["run_stop"] = ("RUN / STOP", "RODAR / PARAR"),
        ["run_stop_tooltip"] = ("Start or freeze the acquisition (Space)", "Inicia ou congela a aquisição (Espaço)"),
        ["single"] = ("SINGLE", "ÚNICO"),
        ["single_tooltip"] = ("Capture one sweep on the next trigger, then stop (S)", "Captura uma varredura no próximo disparo e para (S)"),
        ["auto_set"] = ("AUTO SET", "AUTOAJUSTE"),
        ["auto_set_tooltip"] = ("Analyse the inputs and pick sensible settings (A)", "Analisa as entradas e escolhe ajustes adequados (A)"),
        ["default"] = ("DEFAULT", "PADRÃO"),
        ["default_tooltip"] = ("Restore the factory setup of the scope and the generator (D)", "Restaura a configuração de fábrica do osciloscópio e do gerador (D)"),

        ["horizontal"] = ("Horizontal", "Horizontal"),
        ["time_per_division"] = ("Time/div", "Tempo/div"),
        ["time_per_division_tooltip"] = ("Horizontal scale: time per grid division. You can also scroll over the screen.",
            "Escala horizontal: tempo por divisão da grade. Também dá para rolar o mouse sobre a tela."),
        ["position"] = ("Position", "Posição"),
        ["horizontal_position_tooltip"] = ("Moves the trigger point horizontally. You can also drag the screen. Double-click to centre.",
            "Move o ponto de disparo na horizontal. Também dá para arrastar a tela. Duplo clique centraliza."),
        ["horizontal_hint"] = ("Scroll over the screen to zoom in time, drag it to pan. Slow timebases switch to roll mode.",
            "Role o mouse sobre a tela para ampliar no tempo e arraste para deslocar. Bases de tempo lentas entram no modo roll."),

        ["vertical"] = ("Vertical", "Vertical"),
        ["channel_tooltip"] = ("Show or hide {0} ({1})", "Mostra ou oculta o {0} ({1})"),
        ["volts_per_division"] = ("Volts/div", "Volts/div"),
        ["volts_per_division_tooltip"] = ("Vertical scale of {0}: volts per grid division.", "Escala vertical do {0}: volts por divisão da grade."),
        ["channel_position_tooltip"] = ("Moves {0} up or down. You can also drag its marker. Double-click to reset.",
            "Move o {0} para cima ou para baixo. Também dá para arrastar o marcador. Duplo clique restaura."),
        ["coupling_tooltip"] = ("DC passes everything, AC blocks the DC offset, GND shows the zero reference.",
            "DC passa tudo, AC bloqueia o nível DC e GND mostra a referência de zero."),

        ["trigger"] = ("Trigger", "Disparo"),
        ["level"] = ("Level", "Nível"),
        ["level_tooltip"] = ("The voltage the signal must cross to start a sweep. You can also drag the T marker.",
            "A tensão que o sinal precisa cruzar para iniciar uma varredura. Também dá para arrastar o marcador T."),
        ["centre_level_tooltip"] = ("Sets the level halfway between the signal's peaks", "Coloca o nível no meio entre os picos do sinal"),
        ["source"] = ("Source", "Fonte"),
        ["source_tooltip"] = ("Which channel the trigger watches", "O canal observado pelo disparo"),
        ["slope"] = ("Slope", "Borda"),
        ["slope_tooltip"] = ("Trigger on rising or falling edges", "Dispara em bordas de subida ou de descida"),
        ["mode"] = ("Mode", "Modo"),
        ["mode_tooltip"] = ("AUTO sweeps even without a trigger, NORMAL waits for one", "AUTO varre mesmo sem disparo, NORMAL espera por um"),

        ["display"] = ("Display", "Tela"),
        ["format"] = ("Format", "Formato"),
        ["format_tooltip"] = ("YT plots voltage over time, XY plots CH1 against CH2 (X)", "YT mostra a tensão no tempo, XY mostra o CH1 contra o CH2 (X)"),
        ["math"] = ("Math", "Mat"),
        ["math_off"] = ("OFF", "DESL"),
        ["math_tooltip"] = ("Combines both channels into a third trace (M)", "Combina os dois canais num terceiro traço (M)"),
        ["persistence"] = ("Persistence", "Persistência"),
        ["persistence_minimum"] = ("MIN", "MÍN"),
        ["persistence_tooltip"] = ("How long the phosphor keeps glowing after the beam passes", "Por quanto tempo o fósforo continua brilhando depois que o feixe passa"),
        ["intensity"] = ("Intensity", "Intensidade"),
        ["intensity_tooltip"] = ("Beam brightness", "Brilho do feixe"),
        ["shortcuts"] = ("Shortcuts: Space run/stop, S single, A auto set, X toggle XY, M math, D default, 1/2 channels, F1 about.",
            "Atalhos: Espaço rodar/parar, S único, A autoajuste, X alterna XY, M math, D padrão, 1/2 canais, F1 sobre."),
        ["language_tooltip"] = ("Interface language", "Idioma da interface"),
    };

    public static LocalisableString RunStop => TABLE.Get("run_stop");
    public static LocalisableString RunStopTooltip => TABLE.Get("run_stop_tooltip");
    public static LocalisableString Single => TABLE.Get("single");
    public static LocalisableString SingleTooltip => TABLE.Get("single_tooltip");
    public static LocalisableString AutoSet => TABLE.Get("auto_set");
    public static LocalisableString AutoSetTooltip => TABLE.Get("auto_set_tooltip");
    public static LocalisableString Default => TABLE.Get("default");
    public static LocalisableString DefaultTooltip => TABLE.Get("default_tooltip");

    public static LocalisableString Horizontal => TABLE.Get("horizontal");
    public static LocalisableString TimePerDivision => TABLE.Get("time_per_division");
    public static LocalisableString TimePerDivisionTooltip => TABLE.Get("time_per_division_tooltip");
    public static LocalisableString Position => TABLE.Get("position");
    public static LocalisableString HorizontalPositionTooltip => TABLE.Get("horizontal_position_tooltip");
    public static LocalisableString HorizontalHint => TABLE.Get("horizontal_hint");

    public static LocalisableString Vertical => TABLE.Get("vertical");
    public static LocalisableString ChannelTooltip(string channel, int key) => TABLE.Get("channel_tooltip", channel, key);
    public static LocalisableString VoltsPerDivision => TABLE.Get("volts_per_division");
    public static LocalisableString VoltsPerDivisionTooltip(string channel) => TABLE.Get("volts_per_division_tooltip", channel);
    public static LocalisableString ChannelPositionTooltip(string channel) => TABLE.Get("channel_position_tooltip", channel);
    public static LocalisableString CouplingTooltip => TABLE.Get("coupling_tooltip");

    public static LocalisableString Trigger => TABLE.Get("trigger");
    public static LocalisableString Level => TABLE.Get("level");
    public static LocalisableString LevelTooltip => TABLE.Get("level_tooltip");
    public static LocalisableString CentreLevelTooltip => TABLE.Get("centre_level_tooltip");
    public static LocalisableString Source => TABLE.Get("source");
    public static LocalisableString SourceTooltip => TABLE.Get("source_tooltip");
    public static LocalisableString Slope => TABLE.Get("slope");
    public static LocalisableString SlopeTooltip => TABLE.Get("slope_tooltip");
    public static LocalisableString Mode => TABLE.Get("mode");
    public static LocalisableString ModeTooltip => TABLE.Get("mode_tooltip");

    public static LocalisableString Display => TABLE.Get("display");
    public static LocalisableString Format => TABLE.Get("format");
    public static LocalisableString FormatTooltip => TABLE.Get("format_tooltip");
    public static LocalisableString Math => TABLE.Get("math");
    public static LocalisableString MathOff => TABLE.Get("math_off");
    public static LocalisableString MathTooltip => TABLE.Get("math_tooltip");
    public static LocalisableString Persistence => TABLE.Get("persistence");
    public static LocalisableString PersistenceMinimum => TABLE.Get("persistence_minimum");
    public static LocalisableString PersistenceTooltip => TABLE.Get("persistence_tooltip");
    public static LocalisableString Intensity => TABLE.Get("intensity");
    public static LocalisableString IntensityTooltip => TABLE.Get("intensity_tooltip");
    public static LocalisableString Shortcuts => TABLE.Get("shortcuts");
    public static LocalisableString LanguageTooltip => TABLE.Get("language_tooltip");
}
