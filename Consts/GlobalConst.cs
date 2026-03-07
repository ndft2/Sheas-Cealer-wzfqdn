using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;

namespace Sheas_Cealer.Consts;

internal abstract class GlobalConst : GlobalMultilangConst
{
    internal static Dictionary<string, string> ThemeColorDictionary => new()
    {
        { _ThemeColorRedName, nameof(MaterialDesignColor.Red) },
        { _ThemeColorYellowName, nameof(MaterialDesignColor.Yellow) },
        { _ThemeColorBlueName, nameof(MaterialDesignColor.Blue) },
        { _ThemeColorGreenName, nameof(MaterialDesignColor.Green) },
        { _ThemeColorOrangeName, nameof(MaterialDesignColor.Orange) }
    };
    internal static Dictionary<string, BaseTheme> ThemeStateDictionary => new()
    {
        { _ThemeStateInheritName, BaseTheme.Inherit },
        { _ThemeStateLightName, BaseTheme.Light },
        { _ThemeStateDarkName, BaseTheme.Dark }
    };
    internal static Dictionary<string, string?> LangOptionDictionary => new()
    {
        { _LangOptionInheritName, null },
        { _LangOptionEnglishName, "en" },
        { _LangOptionChineseName, "zh" }
    };
    public static ObservableCollection<string> DefaultBrowserPathCollection => new(new[]
    {
        Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\msedge.exe")?.GetValue(string.Empty, null)?.ToString(),
        Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\chrome.exe")?.GetValue(string.Empty, null)?.ToString(),
        Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\App Paths\\brave.exe")?.GetValue(string.Empty, null)?.ToString()
    }.Where(browserPath => !string.IsNullOrEmpty(browserPath))!);
    internal static string NginxConfPath => Path.Combine(MainConst.AppBasePath, "nginx.conf");
    internal static string ClashConfPath => Path.Combine(MainConst.AppBasePath, "config.yaml");
    internal static string HostsConfPath => Path.Combine(Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Services\\Tcpip\\Parameters\\DataBasePath")?.GetValue("DataBasePath", null)?.ToString() ?? "C:\\Windows\\System32\\drivers\\etc", "hosts");
    internal static string GithubReleaseUrl => "https://github.com/SpaceTimee/Sheas-Cealer/releases/latest";
    public static string DefaultUpstreamUrl => "https://gitlab.com/SpaceTimee/Cealing-Host/raw/main/Cealing-Host.json";
    public static string VersionAboutInfoContent => Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()!.InformationalVersion;
    public static bool IsRunningWithAdminPermisson => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
}