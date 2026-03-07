using MaterialDesignThemes.Wpf;
using System;

namespace Sheas_Cealer.Models;

internal class FlyoutItem
{
    public string Title { get; init; } = string.Empty;
    public PackIcon Icon { get; init; } = null!;
    public Uri Path { get; init; } = null!;
}