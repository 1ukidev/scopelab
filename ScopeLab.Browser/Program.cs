using osu.Framework.Browser;
using ScopeLab;

await BrowserGameHost.InitialiseAsync();

// The host must not be disposed here, as the game keeps running in the browser after Run() returns.
var host = new BrowserGameHost("ScopeLab");

host.Run(new ScopeLabGame());
