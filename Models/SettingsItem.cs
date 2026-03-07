using System.Windows.Controls;

namespace Sheas_Cealer.Models;

internal class SettingsItem
{
    public string Title { get; init; } = string.Empty;
    public Control Control { get; init; } = null!;
    public string? Content { get; init; } = null;
}