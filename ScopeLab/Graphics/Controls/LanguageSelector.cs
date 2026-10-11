using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using ScopeLab.Localisation;

namespace ScopeLab.Graphics.Controls;

/// <summary>
/// Switches the interface language. The choice is stored in the framework configuration.
/// </summary>
public partial class LanguageSelector : OptionSelector<Language>
{
    private Bindable<string> locale = null!;

    public LanguageSelector()
        : base(Enum.GetValues<Language>())
    {
        LabelFor = language => language == Language.Portuguese ? "PT" : "EN";
        TooltipText = ControlStrings.LanguageTooltip;
    }

    [BackgroundDependencyLoader]
    private void load(FrameworkConfigManager config)
    {
        locale = config.GetBindable<string>(FrameworkSetting.Locale);

        Current.Value = LanguageExtensions.FromLocale(locale.Value);
        Current.BindValueChanged(e => locale.Value = e.NewValue.ToLocale());
    }
}
