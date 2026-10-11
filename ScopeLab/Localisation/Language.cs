using System.Globalization;

namespace ScopeLab.Localisation;

public enum Language
{
    English,
    Portuguese,
}

public static class LanguageExtensions
{
    /// <summary>
    /// The locale name the language is registered under, as stored in the framework configuration.
    /// </summary>
    public static string ToLocale(this Language language) => language == Language.Portuguese ? "pt" : "en";

    public static CultureInfo ToCulture(this Language language) => CultureInfo.GetCultureInfo(language == Language.Portuguese ? "pt-BR" : "en-US");

    /// <summary>
    /// Resolves a configured locale. An empty locale follows the operating system's language.
    /// </summary>
    public static Language FromLocale(string locale)
    {
        if (string.IsNullOrEmpty(locale))
            locale = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        return locale.StartsWith("pt") ? Language.Portuguese : Language.English;
    }
}
