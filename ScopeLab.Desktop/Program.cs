using osu.Framework;
using ScopeLab;

using var host = Host.GetSuitableDesktopHost("ScopeLab");
using var game = new ScopeLabGame();

host.Run(game);
