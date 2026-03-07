using MaterialDesignThemes.Wpf;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace Sheas_Cealer.Utils;

internal static partial class WindowThemeManager
{
    private enum DWMWINDOWATTRIBUTE : uint
    {
        DWMWA_USE_IMMERSIVE_DARK_MODE = 20,
        DWMWA_CAPTION_COLOR = 35
    }

    [LibraryImport("dwmapi.dll")]
    private static partial void DwmSetWindowAttribute(nint hwnd, DWMWINDOWATTRIBUTE attr, ref int attrValue, int attrSize);

    internal static void ApplyCurrentTheme(Window window)
    {
        nint windowHandle = new WindowInteropHelper(window).EnsureHandle();
        int useImmersiveDarkMode = new PaletteHelper().GetTheme().GetBaseTheme() == BaseTheme.Light ? 0 : 1;

        DwmSetWindowAttribute(windowHandle, DWMWINDOWATTRIBUTE.DWMWA_USE_IMMERSIVE_DARK_MODE, ref useImmersiveDarkMode, sizeof(int));

        Color captionThemeColor = (window.FindResource(useImmersiveDarkMode == 1 ? "MaterialDesign.Brush.Background" : "MaterialDesign.Brush.Primary.Lighter") as SolidColorBrush)!.Color;
        int captionColorValue = captionThemeColor.R | captionThemeColor.G << 8 | captionThemeColor.B << 16;

        DwmSetWindowAttribute(windowHandle, DWMWINDOWATTRIBUTE.DWMWA_CAPTION_COLOR, ref captionColorValue, sizeof(int));
    }
}