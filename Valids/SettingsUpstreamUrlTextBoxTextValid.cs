using Sheas_Cealer.Consts;
using System.Globalization;
using System.Windows.Controls;

namespace Sheas_Cealer.Valids;

internal class SettingsUpstreamUrlTextBoxTextValid : ValidationRule
{
    public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
    {
        string? upstreamUrl = value as string;

        return new(string.IsNullOrEmpty(upstreamUrl) || SettingsConst.UpstreamUrlRegex().IsMatch(upstreamUrl), SettingsConst._UpstreamUrlInvalidValidationContent);
    }
}