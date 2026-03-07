using Sheas_Cealer.Consts;
using System.Globalization;
using System.IO;
using System.Windows.Controls;

namespace Sheas_Cealer.Valids;

internal class SettingsBrowserPathTextBoxTextValid : ValidationRule
{
    public override ValidationResult Validate(object? value, CultureInfo cultureInfo)
    {
        string? browserPath = value as string;

        return new(File.Exists(browserPath) && Path.GetFileName(browserPath).ToLowerInvariant().EndsWith(".exe"),
            !File.Exists(browserPath) ? SettingsConst._BrowserPathFileInvalidValidationContent : SettingsConst._BrowserPathExtensionInvalidValidationContent);
    }
}