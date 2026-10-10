using osu.Framework;
using ScopeLab.Tests;

using var host = Host.GetSuitableDesktopHost("visual-tests");
using var browser = new ScopeLabTestBrowser();

host.Run(browser);
