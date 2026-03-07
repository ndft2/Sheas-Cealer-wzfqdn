using CommunityToolkit.Mvvm.ComponentModel;
using MaterialDesignThemes.Wpf;
using Sheas_Cealer.Consts;
using Sheas_Cealer.Models;
using Sheas_Cealer.Props;
using Sheas_Cealer.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Sheas_Cealer.Preses;

internal abstract partial class GlobalPres : ObservableObject
{
    internal static event Func<bool, Task>? IsClashHostsActiveChanged;
    internal static event Action<bool>? IsSpeedMonitorEnabledChanged;

    protected GlobalPres()
    {
        themeColorName = !string.IsNullOrEmpty(Settings.Default.ThemeColorName) ?
            GlobalConst.ResourceManager.GetString(Settings.Default.ThemeColorName)! : GlobalConst._ThemeColorRedName;

        themeStateName = !string.IsNullOrEmpty(Settings.Default.ThemeStateName) ?
            GlobalConst.ResourceManager.GetString(Settings.Default.ThemeStateName)! : GlobalConst._ThemeStateInheritName;

        langOptionName = !string.IsNullOrEmpty(Settings.Default.LangOptionName) ?
            GlobalConst.ResourceManager.GetString(Settings.Default.LangOptionName)! : GlobalConst._LangOptionInheritName;

        string[] commandLineArgArray = Environment.GetCommandLineArgs();

        int browserPathIndex = Array.FindIndex(commandLineArgArray, commandLineArg => commandLineArg.Equals("-b", StringComparison.OrdinalIgnoreCase)) + 1;
        int upstreamUrlIndex = Array.FindIndex(commandLineArgArray, commandLineArg => commandLineArg.Equals("-u", StringComparison.OrdinalIgnoreCase)) + 1;
        int extraArgIndex = Array.FindIndex(commandLineArgArray, commandLineArg => commandLineArg.Equals("-e", StringComparison.OrdinalIgnoreCase)) + 1;

        if (browserPathIndex != 0 && browserPathIndex != commandLineArgArray.Length)
            BrowserPath = commandLineArgArray[browserPathIndex];

        if (upstreamUrlIndex != 0 && upstreamUrlIndex != commandLineArgArray.Length)
            UpstreamUrl = commandLineArgArray[upstreamUrlIndex];

        if (extraArgIndex != 0 && extraArgIndex != commandLineArgArray.Length)
            ExtraArgs = commandLineArgArray[extraArgIndex];
    }

    [ObservableProperty]
    private static string themeColorName = string.Empty;
    partial void OnThemeColorNameChanged(string value)
    {
        Settings.Default.ThemeColorName = ResourceKeyFinder.FindGlobalKey(value);
        Settings.Default.Save();

        SnackbarMessageQueue.Enqueue(GlobalConst._ThemeColorRestartToApplySnackbarMsg);
    }

    [ObservableProperty]
    private static string themeStateName = string.Empty;
    partial void OnThemeStateNameChanged(string value)
    {
        PaletteHelper paletteHelper = new();
        Theme customTheme = paletteHelper.GetTheme();

        customTheme.SetBaseTheme(GlobalConst.ThemeStateDictionary[value]);
        paletteHelper.SetTheme(customTheme);

        Settings.Default.ThemeStateName = ResourceKeyFinder.FindGlobalKey(value);
        Settings.Default.Save();
    }

    [ObservableProperty]
    private static string langOptionName = string.Empty;
    partial void OnLangOptionNameChanged(string value)
    {
        Settings.Default.LangOptionName = ResourceKeyFinder.FindGlobalKey(value);
        Settings.Default.Save();

        SnackbarMessageQueue.Enqueue(GlobalConst._LangOptionRestartToApplySnackbarMsg);
    }

    [ObservableProperty]
    private static string browserPath = !string.IsNullOrEmpty(Settings.Default.BrowserPath) ? Settings.Default.BrowserPath : GlobalConst.DefaultBrowserPathCollection.FirstOrDefault() ?? string.Empty;
    partial void OnBrowserPathChanged(string value)
    {
        Settings.Default.BrowserPath = value;
        Settings.Default.Save();
    }

    [ObservableProperty]
    private static string upstreamUrl = Settings.Default.UpstreamUrl;
    partial void OnUpstreamUrlChanged(string value)
    {
        if (value == null)
        {
            UpstreamUrl = string.Empty;

            return;
        }

        IsUpstreamMirrorEnabled = string.IsNullOrEmpty(value) || value.Contains("github.com", StringComparison.OrdinalIgnoreCase) || value.Contains("gitlab.com", StringComparison.OrdinalIgnoreCase);

        Settings.Default.UpstreamUrl = value;
        Settings.Default.Save();
    }

    [ObservableProperty]
    private static string extraArgs = Settings.Default.ExtraArgs;
    partial void OnExtraArgsChanged(string value)
    {
        if (value == null)
        {
            ExtraArgs = string.Empty;

            return;
        }

        Settings.Default.ExtraArgs = value;
        Settings.Default.Save();
    }

    [ObservableProperty]
    private static bool isUpstreamMirrorEnabled = (string.IsNullOrEmpty(upstreamUrl) || upstreamUrl.Contains("github.com", StringComparison.OrdinalIgnoreCase) || upstreamUrl.Contains("gitlab.com", StringComparison.OrdinalIgnoreCase)) && Settings.Default.IsUpstreamMirrorEnabled;
    partial void OnIsUpstreamMirrorEnabledChanged(bool value)
    {
        Settings.Default.IsUpstreamMirrorEnabled = value;
        Settings.Default.Save();
    }

    [ObservableProperty]
    private static bool isUpdateSoftwareEnabled = Settings.Default.IsUpdateSoftwareEnabled;
    partial void OnIsUpdateSoftwareEnabledChanged(bool value)
    {
        Settings.Default.IsUpdateSoftwareEnabled = value;
        Settings.Default.Save();
    }

    [ObservableProperty]
    private static bool isUpdateHostEnabled = Settings.Default.IsUpdateHostEnabled;
    partial void OnIsUpdateHostEnabledChanged(bool value)
    {
        Settings.Default.IsUpdateHostEnabled = value;
        Settings.Default.Save();
    }

    [ObservableProperty]
    private static bool isAlwaysAdminEnabled = Settings.Default.IsAlwaysAdminEnabled;
    partial void OnIsAlwaysAdminEnabledChanged(bool value)
    {
        Settings.Default.IsAlwaysAdminEnabled = value;
        Settings.Default.Save();
    }

    [ObservableProperty]
    private static bool isKeepRunningEnabled = Settings.Default.IsKeepRunningEnabled;
    partial void OnIsKeepRunningEnabledChanged(bool value)
    {
        Settings.Default.IsKeepRunningEnabled = value;
        Settings.Default.Save();
    }

    [ObservableProperty]
    private static bool isClashHostsEnabled = Settings.Default.isClashHostsEnabled;
    partial void OnIsClashHostsEnabledChanged(bool value)
    {
        if (!value)
            IsClashHostsActive = false;

        Settings.Default.isClashHostsEnabled = value;
        Settings.Default.Save();
    }

    [ObservableProperty]
    private static bool isSpeedMonitorEnabled = Settings.Default.IsSpeedMonitorEnabled;
    partial void OnIsSpeedMonitorEnabledChanged(bool value)
    {
        foreach (Action<bool> isSpeedMonitorEnabledChangedFunc in IsSpeedMonitorEnabledChanged!.GetInvocationList())
            isSpeedMonitorEnabledChangedFunc.Invoke(value);

        Settings.Default.IsSpeedMonitorEnabled = value;
        Settings.Default.Save();
    }

    [ObservableProperty]
    private ProcessStatus browserStatus;

    [ObservableProperty]
    private ProcessStatus nginxStatus;
    async partial void OnNginxStatusChanged(ProcessStatus oldValue, ProcessStatus newValue)
    {
        if ((oldValue == default || oldValue == ProcessStatus.Initing || oldValue == ProcessStatus.On) && newValue == ProcessStatus.Off)
        {
            IsClashHostsActive = false;

            string hostsConfContent = await File.ReadAllTextAsync(GlobalConst.HostsConfPath);
            int hostsConfStartIndex = hostsConfContent.IndexOf(MainConst.HostsConfStartMarker, StringComparison.Ordinal);
            int hostsConfEndIndex = hostsConfContent.LastIndexOf(MainConst.HostsConfEndMarker, StringComparison.Ordinal);

            if (hostsConfStartIndex != -1 && hostsConfEndIndex != -1)
                await File.WriteAllTextAsync(GlobalConst.HostsConfPath, hostsConfContent.Remove(hostsConfStartIndex, hostsConfEndIndex - hostsConfStartIndex + MainConst.HostsConfEndMarker.Length));

            using X509Store certStore = new(StoreName.Root, StoreLocation.LocalMachine, OpenFlags.ReadWrite);

            foreach (X509Certificate2 storedCert in certStore.Certificates)
                if (storedCert.Subject == MainConst.RootCertSubjectName)
                    while (true)
                        try
                        {
                            certStore.Remove(storedCert);

                            break;
                        }
                        catch { }

            certStore.Close();
        }
    }

    [ObservableProperty]
    private ProcessStatus clashStatus;
    partial void OnClashStatusChanged(ProcessStatus oldValue, ProcessStatus newValue)
    {
        if ((oldValue == default || oldValue == ProcessStatus.Initing || oldValue == ProcessStatus.On) && newValue == ProcessStatus.Off)
            IsClashHostsActive = false;
    }

    [ObservableProperty]
    private bool isClashHostsActive = Settings.Default.IsClashHostsActive;
    async partial void OnIsClashHostsActiveChanged(bool value)
    {
        List<Task> isClashHostsActiveChangedTaskList = [];

        foreach (Func<bool, Task> isClashHostsActiveChangedFunc in IsClashHostsActiveChanged!.GetInvocationList())
            isClashHostsActiveChangedTaskList.Add(isClashHostsActiveChangedFunc.Invoke(value));

        await Task.WhenAll(isClashHostsActiveChangedTaskList);

        Settings.Default.IsClashHostsActive = value;
        Settings.Default.Save();
    }

    [ObservableProperty]
    private static SnackbarMessageQueue snackbarMessageQueue = new(TimeSpan.FromSeconds(2));
}