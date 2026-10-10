using osu.Framework.Graphics;

namespace ScopeLab.Graphics;

public static class ScopeColours
{
    public static readonly Colour4 BENCH = Colour4.FromHex("0b0d11");
    public static readonly Colour4 BENCH_LIGHT = Colour4.FromHex("161a21");

    public static readonly Colour4 PANEL = Colour4.FromHex("1c2028");
    public static readonly Colour4 PANEL_LIGHT = Colour4.FromHex("262b35");
    public static readonly Colour4 PANEL_DARK = Colour4.FromHex("13161b");
    public static readonly Colour4 BORDER = Colour4.FromHex("323844");

    public static readonly Colour4 KNOB = Colour4.FromHex("3a404c");
    public static readonly Colour4 KNOB_DARK = Colour4.FromHex("1d2027");

    public static readonly Colour4 TEXT = Colour4.FromHex("e8ebf1");
    public static readonly Colour4 TEXT_DIM = Colour4.FromHex("98a0ae");
    public static readonly Colour4 TEXT_FAINT = Colour4.FromHex("5c6371");

    public static readonly Colour4 SCREEN = Colour4.FromHex("05090b");
    public static readonly Colour4 SCREEN_LIGHT = Colour4.FromHex("0c1518");
    public static readonly Colour4 GRATICULE = Colour4.FromHex("7fa6b0");

    public static readonly Colour4 TRIGGER = Colour4.FromHex("ff8f3f");
    public static readonly Colour4 MATH = Colour4.FromHex("ff62d4");
    public static readonly Colour4 XY = Colour4.FromHex("72ff9c");
    public static readonly Colour4 GENERATOR = Colour4.FromHex("8f8cff");

    public static readonly Colour4 RUN = Colour4.FromHex("3ddc84");
    public static readonly Colour4 STOP = Colour4.FromHex("ff4d5e");

    private static readonly Colour4[] channels = [Colour4.FromHex("ffd84d"), Colour4.FromHex("3fd2ff")];

    public static Colour4 ForChannel(int index) => channels[index];
}
