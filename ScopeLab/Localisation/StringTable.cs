using System.Collections.Generic;
using System.Linq;
using osu.Framework.Localisation;

namespace ScopeLab.Localisation;

/// <summary>
/// A group of interface strings, each pairing its English text (which doubles as the fallback) with its Portuguese translation.
/// Keeping both side by side makes a missing or stale translation easy to spot.
/// </summary>
public class StringTable(string scope)
{
    private readonly Dictionary<string, (string English, string Portuguese)> entries = new Dictionary<string, (string English, string Portuguese)>();

    public (string English, string Portuguese) this[string key]
    {
        set => entries.Add(key, value);
    }

    /// <summary>
    /// The localisable string for an entry. Arguments fill in its <c>{0}</c>-style placeholders.
    /// </summary>
    public LocalisableString Get(string key, params object?[] args) => new TranslatableString(qualify(key), entries[key].English, args);

    public IEnumerable<(string Key, string English, string Portuguese)> Entries =>
        entries.Select(e => (qualify(e.Key), e.Value.English, e.Value.Portuguese));

    private string qualify(string key) => $"{scope}:{key}";
}
