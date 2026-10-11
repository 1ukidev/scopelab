using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using osu.Framework.Localisation;
using ScopeLab.Localisation;

namespace ScopeLab.Tests.NonVisual;

[TestFixture]
public partial class LocalisationTest
{
    [Test]
    public void TestEveryEntryIsTranslated()
    {
        foreach (var (key, english, portuguese) in ScopeLocalisationStore.TABLES.SelectMany(t => t.Entries))
        {
            Assert.Multiple(() =>
            {
                Assert.That(english, Is.Not.Empty, key);
                Assert.That(portuguese, Is.Not.Empty, key);
                Assert.That(placeholders(portuguese), Is.EquivalentTo(placeholders(english)), $"Placeholders differ in {key}");
            });
        }
    }

    [Test]
    public void TestKeysAreUnique()
    {
        var keys = ScopeLocalisationStore.TABLES.SelectMany(t => t.Entries).Select(e => e.Key).ToArray();
        Assert.That(keys, Is.Unique);
    }

    [Test]
    public void TestStoresServeTheirLanguage()
    {
        var english = new ScopeLocalisationStore(Language.English);
        var portuguese = new ScopeLocalisationStore(Language.Portuguese);

        Assert.Multiple(() =>
        {
            Assert.That(english.Get("controls:default"), Is.EqualTo("DEFAULT"));
            Assert.That(portuguese.Get("controls:default"), Is.EqualTo("PADRÃO"));
            Assert.That(portuguese.EffectiveCulture.Name, Is.EqualTo("pt-BR"));
            Assert.That(portuguese.Get("controls:missing"), Is.Null);
        });
    }

    [Test]
    public void TestEveryStringPropertyResolves()
    {
        var properties = typeof(ControlStrings).Assembly.GetTypes()
                                               .Where(t => t.Namespace == typeof(ControlStrings).Namespace)
                                               .SelectMany(t => t.GetProperties(BindingFlags.Public | BindingFlags.Static))
                                               .Where(p => p.PropertyType == typeof(LocalisableString));

        foreach (var property in properties)
            Assert.DoesNotThrow(() => property.GetValue(null), $"{property.DeclaringType!.Name}.{property.Name}");
    }

    [TestCase("pt", Language.Portuguese)]
    [TestCase("pt-BR", Language.Portuguese)]
    [TestCase("en", Language.English)]
    [TestCase("de", Language.English)]
    public void TestLocaleResolution(string locale, Language expected) => Assert.That(LanguageExtensions.FromLocale(locale), Is.EqualTo(expected));

    [Test]
    public void TestLocaleRoundTrip()
    {
        foreach (var language in new[] { Language.English, Language.Portuguese })
            Assert.That(LanguageExtensions.FromLocale(language.ToLocale()), Is.EqualTo(language));
    }

    private static string[] placeholders(string text) => placeholderPattern().Matches(text).Select(m => m.Value).Distinct().ToArray();

    [GeneratedRegex(@"\{\d+\}")]
    private static partial Regex placeholderPattern();
}
