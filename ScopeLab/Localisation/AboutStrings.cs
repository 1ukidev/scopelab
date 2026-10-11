using osu.Framework.Localisation;

namespace ScopeLab.Localisation;

/// <summary>
/// Texts of the "about" dialog.
/// </summary>
public static class AboutStrings
{
    public static readonly StringTable TABLE = new StringTable("about")
    {
        ["open_tooltip"] = ("About ScopeLab (F1)", "Sobre o ScopeLab (F1)"),
        ["close_tooltip"] = ("Close (Esc)", "Fechar (Esc)"),
        ["subtitle"] = ("Oscilloscope & function generator simulator", "Simulador de osciloscópio e gerador de funções"),
        ["introduction"] = ("ScopeLab is a bench oscilloscope simulator. A dual-channel digital oscilloscope is wired to a two-output "
                            + "function generator, so you can create signals and examine them just like on a real lab bench.",
            "O ScopeLab é um simulador de osciloscópio de bancada. Um osciloscópio digital de dois canais está ligado a um gerador "
            + "de funções com duas saídas, para você criar sinais e examiná-los como numa bancada de laboratório de verdade."),
        ["controls"] = ("Pick waveforms, frequencies and amplitudes on the generator, then use the timebase, vertical and trigger "
                        + "controls to get a stable picture. Explore XY figures, math traces, persistence, automatic measurements "
                        + "and the frequency counter.",
            "Escolha formas de onda, frequências e amplitudes no gerador e use os controles de tempo, vertical e disparo para "
            + "obter uma imagem estável. Explore figuras XY, traços matemáticos, persistência, medições automáticas e o "
            + "contador de frequência."),
        ["phosphor"] = ("The screen imitates a phosphor display: the beam glows brighter where it moves slowly, fades on fast "
                        + "edges and leaves an afterglow behind.",
            "A tela imita um display de fósforo: o feixe brilha mais onde se move devagar, some nas bordas rápidas e deixa "
            + "um rastro para trás."),
        ["tips"] = ("Hover any control for a hint.\n"
                    + "Drag or scroll knobs, hold Shift for fine tuning and double-click to reset.\n"
                    + "Drag the markers on the screen edges, scroll over the screen to zoom in time.\n"
                    + "AUTO SET finds a picture for you, DEFAULT restores the factory setup.",
            "Passe o mouse sobre qualquer controle para ver uma dica.\n"
            + "Arraste ou role os knobs, segure Shift para ajuste fino e dê duplo clique para restaurar.\n"
            + "Arraste os marcadores nas bordas da tela e role o mouse sobre ela para ampliar no tempo.\n"
            + "O AUTOAJUSTE encontra uma imagem para você, e o PADRÃO restaura a configuração de fábrica."),
        ["built_with"] = ("Built with osu!framework", "Feito com osu!framework"),
        ["repository_tooltip"] = ("Open the source code on GitHub", "Abrir o código-fonte no GitHub"),
    };

    public static LocalisableString OpenTooltip => TABLE.Get("open_tooltip");
    public static LocalisableString CloseTooltip => TABLE.Get("close_tooltip");
    public static LocalisableString Subtitle => TABLE.Get("subtitle");
    public static LocalisableString Introduction => TABLE.Get("introduction");
    public static LocalisableString Controls => TABLE.Get("controls");
    public static LocalisableString Phosphor => TABLE.Get("phosphor");
    public static LocalisableString Tips => TABLE.Get("tips");
    public static LocalisableString BuiltWith => TABLE.Get("built_with");
    public static LocalisableString RepositoryTooltip => TABLE.Get("repository_tooltip");
}
