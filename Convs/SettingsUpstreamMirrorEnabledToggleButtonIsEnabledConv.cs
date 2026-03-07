using System;
using System.Globalization;
using System.Windows.Data;

namespace Sheas_Cealer.Convs;

internal class SettingsUpstreamMirrorEnabledToggleButtonIsEnabledConv : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string? upstreamUrl = value as string;

        return string.IsNullOrEmpty(upstreamUrl) || upstreamUrl.Contains("github.com", StringComparison.OrdinalIgnoreCase) || upstreamUrl.Contains("gitlab.com", StringComparison.OrdinalIgnoreCase);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
}