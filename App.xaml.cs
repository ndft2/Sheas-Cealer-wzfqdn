using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using Sheas_Cealer.Consts;
using Sheas_Cealer.Preses;
using Sheas_Cealer.Props;
using Sheas_Cealer.Utils;
using Sheas_Cealer.Wins;
using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Sheas_Cealer;

internal partial class App : Application
{
    private readonly AppPres AppPres;

    internal App()
    {
        InitializeComponent();

        AppPres = new();

        if (Settings.Default.IsUpgradeRequired)
        {
            Settings.Default.Upgrade();

            Settings.Default.IsUpgradeRequired = false;
            Settings.Default.Save();
        }

        PaletteHelper paletteHelper = new();

        paletteHelper.GetThemeManager()!.ThemeChanged += App_ThemeChanged;

        Theme customTheme = paletteHelper.GetTheme();

        if (GlobalConst.ThemeColorDictionary.TryGetValue(AppPres.ThemeColorName, out string? themeColorName))
        {
            Resources.MergedDictionaries.Add(new() { Source = new($"pack://application:,,,/MaterialDesignColors;component/Themes/MaterialDesignColor.{themeColorName}.Primary.xaml") });
            Resources.MergedDictionaries.Add(new() { Source = new($"pack://application:,,,/MaterialDesignColors;component/Themes/MaterialDesignColor.{themeColorName}.Secondary.xaml") });

            customTheme.SetPrimaryColor(SwatchHelper.Lookup[Enum.Parse<MaterialDesignColor>(themeColorName)]);
            customTheme.SetSecondaryColor(SwatchHelper.Lookup[Enum.Parse<MaterialDesignColor>(themeColorName)]);
        }

        if (GlobalConst.ThemeStateDictionary.TryGetValue(AppPres.ThemeStateName, out BaseTheme themeStateTheme))
            customTheme.SetBaseTheme(themeStateTheme);

        if (GlobalConst.LangOptionDictionary.TryGetValue(AppPres.LangOptionName, out string? langOptionCulture))
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.DefaultThreadCurrentUICulture = langOptionCulture == null ? null! : new(langOptionCulture);

        paletteHelper.SetTheme(customTheme);
    }
    private async void App_OnStartup(object sender, StartupEventArgs e)
    {
        if (!GlobalConst.IsRunningWithAdminPermisson && AppPres.IsAlwaysAdminEnabled)
            AdminPermissionElevator.RestartWithAdminPermission();

        new MainWin().Show();

        MainWindow.Activate();
    }

    private void App_ThemeChanged(object? sender, ThemeChangedEventArgs e)
    {
        foreach (ResourceDictionary mergedDictionary in Resources.MergedDictionaries)
            if (mergedDictionary.Source?.ToString() == $"Rsces/Themes/LightTheme.xaml" || mergedDictionary.Source?.ToString() == $"Rsces/Themes/DarkTheme.xaml")
            {
                Resources.MergedDictionaries.Remove(mergedDictionary);

                break;
            }

        Resources.MergedDictionaries.Add(new() { Source = new($"Rsces/Themes/{new PaletteHelper().GetTheme().GetBaseTheme()}Theme.xaml", UriKind.Relative) });

        foreach (Window currentWindow in Windows)
            WindowThemeManager.ApplyCurrentTheme(currentWindow);
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        AppPres.SnackbarMessageQueue.Enqueue(string.Format(AppConst._ErrorSnackbarMsg, e.Exception.Message), AppConst._ErrorSnackbarButtonContent, _ => Clipboard.SetText(e.Exception.Message), null, true, false, TimeSpan.FromSeconds(3.5));

        e.Handled = true;
    }
}