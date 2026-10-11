using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Localisation;

namespace ScopeLab.Localisation;

/// <summary>
/// Serves the interface strings of one <see cref="Language"/> to the framework's localisation system.
/// </summary>
public class ScopeLocalisationStore : ILocalisationStore
{
    public static readonly StringTable[] TABLES = [ControlStrings.TABLE, GeneratorStrings.TABLE, DisplayStrings.TABLE, AboutStrings.TABLE];

    private readonly Dictionary<string, string> strings;

    public ScopeLocalisationStore(Language language)
    {
        EffectiveCulture = language.ToCulture();
        strings = TABLES.SelectMany(t => t.Entries)
                        .ToDictionary(e => e.Key, e => language == Language.Portuguese ? e.Portuguese : e.English);
    }

    public CultureInfo EffectiveCulture { get; }

    public string? Get(string name) => strings.GetValueOrDefault(name);

    public Task<string?> GetAsync(string name, CancellationToken cancellationToken = default) => Task.FromResult(Get(name));

    public Stream? GetStream(string name) => null;

    public IEnumerable<string> GetAvailableResources() => strings.Keys;

    public void Dispose()
    {
    }
}
