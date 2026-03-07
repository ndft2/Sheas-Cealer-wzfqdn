using Sheas_Cealer.Consts;
using System.Globalization;
using System.Windows.Controls;

namespace Sheas_Cealer.Valids;

internal class SettingsExtraArgsTextBoxTextValid : ValidationRule
{
    public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
    {
        string? extraArgs = value as string;

        return new(string.IsNullOrEmpty(extraArgs) || SettingsConst.ExtraArgsRegex().IsMatch(extraArgs), SettingsConst._ExtraArgsInvalidValidationContent);
    }
}