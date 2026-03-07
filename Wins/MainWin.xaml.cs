using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using NginxConfigParser;
using Ona_Core;
using Sheas_Cealer.Consts;
using Sheas_Cealer.Exts;
using Sheas_Cealer.Models;
using Sheas_Cealer.Preses;
using Sheas_Cealer.Proces;
using Sheas_Cealer.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Design;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;
using YamlDotNet.Core.Tokens;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using File = System.IO.File;
using Settings = Sheas_Cealer.Props.Settings;

namespace Sheas_Cealer.Wins;

internal partial class MainWin : Window
{
    private readonly MainPres MainPres;
    private readonly HttpClient MainClient = new(new HttpClientHandler { ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator });
    private readonly FileSystemWatcher CealHostWatcher = new(Path.GetDirectoryName(MainConst.CealHostPath)!, Path.GetFileName(MainConst.CealHostPath)) { NotifyFilter = NotifyFilters.LastWrite };
    private readonly FileSystemWatcher NginxConfWatcher = new(Path.GetDirectoryName(GlobalConst.NginxConfPath)!, Path.GetFileName(GlobalConst.NginxConfPath)) { NotifyFilter = NotifyFilters.LastWrite };
    private readonly FileSystemWatcher ClashConfWatcher = new(Path.GetDirectoryName(GlobalConst.ClashConfPath)!, Path.GetFileName(GlobalConst.ClashConfPath)) { NotifyFilter = NotifyFilters.LastWrite };
    private readonly DispatcherTimer BrowserStatusTimer = new() { Interval = TimeSpan.FromSeconds(0.1) };
    private readonly DispatcherTimer NginxStatusTimer = new() { Interval = TimeSpan.FromSeconds(0.1) };
    private readonly DispatcherTimer NginxPortTimer = new() { Interval = TimeSpan.FromSeconds(0.1) };
    private readonly DispatcherTimer ClashStatusTimer = new() { Interval = TimeSpan.FromSeconds(0.1) };
    private readonly DispatcherTimer ClashPortTimer = new() { Interval = TimeSpan.FromSeconds(0.1) };
    private readonly DispatcherTimer TrafficSpeedTimer = new() { Interval = TimeSpan.FromSeconds(1) };

    private long LastReceivedBytes = 0;
    private long LastSentBytes = 0;

    private readonly SortedDictionary<string, List<CealHostRule?>?> CealHostRulesDict = [];
    private string CealArgs = string.Empty;

    private NginxConfig? NginxConf = null;
    private string NginxExtraConf = string.Empty;
    private int NginxHttpPort = MainConst.NginxDefaultHttpPort;
    private int NginxHttpsPort = MainConst.NginxDefaultHttpsPort;

    private ClashConfig? ClashConf = null;
    private string ClashExtraConf = string.Empty;
    private readonly Dictionary<string, object> ClashConfHostsDict = [];
    private int ClashMixedPort = MainConst.ClashDefaultMixedPort;

    private string HostsConf = string.Empty;

    private RSA CertKey = null!;
    private X509Certificate2 RootCert = null!;
    private X509Certificate2 ChildCert = null!;

    internal MainWin()
    {
        InitializeComponent();

        DataContext = MainPres = new(RulesFlyoutItem);
    }
    private void MainWin_SourceInitialized(object sender, EventArgs e) => WindowThemeManager.ApplyCurrentTheme(this);
    private async void MainWin_Loaded(object sender, RoutedEventArgs e)
    {
        await Task.Run(async () =>
        {
            if (Settings.Default.IsExpandBarOpen)
                MainPres.IsExpandBarOpen = true;

            TrafficSpeedTimer.Tick += TrafficSpeedTimer_Tick;

            if (MainPres.IsSpeedMonitorEnabled)
                TrafficSpeedTimer.Start();

            MainPres.IsSpeedMonitorEnabledChanged += MainPres_IsSpeedMonitorEnabledChanged;

            Directory.CreateDirectory(MainConst.AppBasePath);

            if (!File.Exists(MainConst.UpstreamHostPath))
                await File.Create(MainConst.UpstreamHostPath).DisposeAsync();
            if (!File.Exists(MainConst.LocalHostPath))
                await File.Create(MainConst.LocalHostPath).DisposeAsync();

            CealHostWatcher.Changed += CealHostWatcher_Changed;
            CealHostWatcher.Created += CealHostWatcher_Changed;
            CealHostWatcher.Deleted += CealHostWatcher_Changed;
            CealHostWatcher.Renamed += CealHostWatcher_Renamed;
            CealHostWatcher.Error += FileSystemWatcher_Error;
            CealHostWatcher.Start();

            BrowserStatusTimer.Tick += BrowserStatusTimer_Tick;
            BrowserStatusTimer.Start();

            if (GlobalConst.IsRunningWithAdminPermisson)
            {
                if (!File.Exists(GlobalConst.NginxConfPath))
                    await File.Create(GlobalConst.NginxConfPath).DisposeAsync();

                NginxConfWatcher.Changed += NginxConfWatcher_Changed;
                NginxConfWatcher.Created += NginxConfWatcher_Changed;
                NginxConfWatcher.Deleted += NginxConfWatcher_Changed;
                NginxConfWatcher.Renamed += NginxConfWatcher_Renamed;
                NginxConfWatcher.Error += FileSystemWatcher_Error;

                NginxPortTimer.Tick += NginxPortTimer_Tick;
                NginxStatusTimer.Tick += NginxStatusTimer_Tick;
                NginxStatusTimer.Start();

                if (!File.Exists(GlobalConst.ClashConfPath))
                    await File.Create(GlobalConst.ClashConfPath).DisposeAsync();

                ClashConfWatcher.Changed += ClashConfWatcher_Changed;
                ClashConfWatcher.Created += ClashConfWatcher_Changed;
                ClashConfWatcher.Deleted += ClashConfWatcher_Changed;
                ClashConfWatcher.Renamed += ClashConfWatcher_Renamed;
                ClashConfWatcher.Error += FileSystemWatcher_Error;

                ClashPortTimer.Tick += ClashPortTimer_Tick;
                ClashStatusTimer.Tick += ClashStatusTimer_Tick;
                ClashStatusTimer.Start();

                MainPres.IsClashHostsActiveChanged += MainPres_IsClashHostsActiveChanged;
            }

            foreach (string cealHostPath in Directory.GetFiles(CealHostWatcher.Path, CealHostWatcher.Filter))
                CealHostWatcher_Changed(null!, new(new(), Path.GetDirectoryName(cealHostPath)!, Path.GetFileName(cealHostPath)));

            if (Array.Exists(Environment.GetCommandLineArgs(), arg => arg.Equals("-s", StringComparison.OrdinalIgnoreCase)))
                BrowserLaunchButton_Click(null, null!);

            if (MainPres.IsUpdateHostEnabled)
                UpdateHostButton_Click(null, null!);

            if (MainPres.IsUpdateSoftwareEnabled)
                UpdateSoftwareButton_Click(null, null!);
        });
    }

    private void MainPres_IsSpeedMonitorEnabledChanged(bool arg)
    {
        if (arg)
        {
            TrafficSpeedTimer.Start();

            MainPres.DownloadSpeed = MainConst.DefaultOnDownloadSpeed;
            MainPres.UploadSpeed = MainConst.DefaultOnUploadSpeed;
        }
        else
        {
            TrafficSpeedTimer.Stop();

            MainPres.DownloadSpeed = MainConst.DefaultOffDownloadSpeed;
            MainPres.UploadSpeed = MainConst.DefaultOffUploadSpeed;
        }
    }

    private void TrafficSpeedTimer_Tick(object? sender, EventArgs e)
    {
        long receivedBytes = 0;
        long sentBytes = 0;

        foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (networkInterface.OperationalStatus != OperationalStatus.Up || networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                continue;

            receivedBytes += networkInterface.GetIPStatistics().BytesReceived;
            sentBytes += networkInterface.GetIPStatistics().BytesSent;
        }

        if (LastReceivedBytes != 0 || LastSentBytes != 0)
        {
            long downloadSpeed = receivedBytes - LastReceivedBytes;
            long uploadSpeed = sentBytes - LastSentBytes;

            int downloadSpeedUnitsArrayIndex = downloadSpeed >= 1024 ? (int)Math.Log(downloadSpeed, 1024) : 0;
            int uploadSpeedUnitsArrayIndex = uploadSpeed >= 1024 ? (int)Math.Log(uploadSpeed, 1024) : 0;

            MainPres.DownloadSpeed = $"↓ {Math.Ceiling(downloadSpeed / Math.Pow(1024, downloadSpeedUnitsArrayIndex) * 10) / 10} {MainConst.TrafficSpeedUnitsArray[downloadSpeedUnitsArrayIndex]}";
            MainPres.UploadSpeed = $"↑ {Math.Ceiling(uploadSpeed / Math.Pow(1024, uploadSpeedUnitsArrayIndex) * 10) / 10} {MainConst.TrafficSpeedUnitsArray[uploadSpeedUnitsArrayIndex]}";
        }

        LastReceivedBytes = receivedBytes;
        LastSentBytes = sentBytes;
    }

    private async void MainWin_Closing(object sender, CancelEventArgs e)
    {
        if (MainPres.NginxStatus == ProcessStatus.Initing)
            await File.WriteAllTextAsync(GlobalConst.NginxConfPath, NginxExtraConf);

        if (MainPres.ClashStatus == ProcessStatus.Initing)
            await File.WriteAllTextAsync(GlobalConst.ClashConfPath, ClashExtraConf);

        Application.Current.Shutdown();
    }

    private async void BrowserLaunchButton_Click(object? sender, RoutedEventArgs e)
    {
        if (CealHostRulesDict.Values.Any(cealHostRules => cealHostRules == null || cealHostRules.Any(cealHostRule => cealHostRule == null)) && MessageBox.Show(MainConst._CealHostInvalidMsg, string.Empty, MessageBoxButton.YesNo) != MessageBoxResult.Yes ||
            sender is not true && MessageBox.Show(MainConst._BrowserProcessKillPrompt, string.Empty, MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            return;

        await ProcessKiller.Kill(MainPres.BrowserPath);

        await Task.Run(() => new BrowserProc(MainPres.BrowserPath, sender is bool).Run(Path.GetDirectoryName(MainPres.BrowserPath)!, $"{CealArgs} {MainPres.ExtraArgs.Trim()}"));
    }
    private void BrowserPinButton_Click(object sender, RoutedEventArgs e) => MainPres.TopBarArray = [0, 1, 2];
    private async void NginxLaunchButton_Click(object sender, RoutedEventArgs e)
    {
        if (MainPres.NginxStatus == ProcessStatus.On)
        {
            await ProcessKiller.Kill(MainConst.NginxPath);

            return;
        }

        if (CealHostRulesDict.Values.Any(cealHostRules => cealHostRules == null || cealHostRules.Any(cealHostRule => cealHostRule == null)) && MessageBox.Show(MainConst._CealHostInvalidMsg, string.Empty, MessageBoxButton.YesNo) != MessageBoxResult.Yes ||
            NginxConf == null && MessageBox.Show(MainConst._NginxConfInvalidMsg, string.Empty, MessageBoxButton.YesNo) != MessageBoxResult.Yes ||
            MainPres.IsClashHostsEnabled && ClashConf == null && MessageBox.Show(MainConst._ClashConfInvalidMsg, string.Empty, MessageBoxButton.YesNo) != MessageBoxResult.Yes ||
            NginxHttpsPort != MainConst.NginxDefaultHttpsPort && MessageBox.Show(string.Format(MainConst._NginxHttpsPortOccupiedMsg, NginxHttpsPort), string.Empty, MessageBoxButton.YesNo) != MessageBoxResult.Yes ||
            NginxHttpPort != MainConst.NginxDefaultHttpPort && MessageBox.Show(string.Format(MainConst._NginxHttpPortOccupiedMsg, NginxHttpPort), string.Empty, MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            return;

        MainPres.SnackbarMessageQueue.Enqueue(MainConst._ProxyLaunchPrompt);

        MainPres.NginxStatus = ProcessStatus.Initing;
        MainPres.IsClashHostsActive = true;

        await ProcessKiller.Kill(MainConst.NginxPath);

        if (!File.Exists(GlobalConst.NginxConfPath))
            await File.Create(GlobalConst.NginxConfPath).DisposeAsync();

        Directory.CreateDirectory(MainConst.NginxLogsPath);
        Directory.CreateDirectory(MainConst.NginxTempPath);

        using X509Store certStore = new(StoreName.Root, StoreLocation.LocalMachine, OpenFlags.ReadWrite);

        certStore.Add(RootCert);
        certStore.Close();

        await File.WriteAllTextAsync(MainConst.NginxCertPath, ChildCert.ExportCertificatePem());
        await File.WriteAllTextAsync(MainConst.NginxKeyPath, CertKey.ExportPkcs8PrivateKeyPem());

        if (MainPres.IsClashHostsEnabled)
        {
            File.SetAttributes(GlobalConst.HostsConfPath, File.GetAttributes(GlobalConst.HostsConfPath) & ~FileAttributes.ReadOnly);

            await File.AppendAllTextAsync(GlobalConst.HostsConfPath, HostsConf);
        }

        NginxConfWatcher.Stop();

        NginxConf!.Save(GlobalConst.NginxConfPath);

        await Task.Run(() =>
        {
            new NginxProc().Run(Path.GetDirectoryName(MainConst.NginxPath)!, @$"-c ""{Path.GetRelativePath(Path.GetDirectoryName(MainConst.NginxPath)!, GlobalConst.NginxConfPath)}""");

            MainPres.NginxStatus = ProcessStatus.Initing;
        });

        if (!MainPres.IsClashHostsEnabled)
            return;
    }
    private void NginxPinButton_Click(object sender, RoutedEventArgs e) => MainPres.TopBarArray = [1, 2, 0];
    private async void ClashLaunchButton_Click(object sender, RoutedEventArgs e)
    {
        await Task.Run(() => new ClashProc().Run(Path.GetDirectoryName(MainConst.ClashPath)!, @$"-d ""{Path.GetDirectoryName(GlobalConst.ClashConfPath)}"""));
    }
    private void ClashPinButton_Click(object sender, RoutedEventArgs e) => MainPres.TopBarArray = [2, 1, 0];

    private async Task MainPres_IsClashHostsActiveChanged(bool isClashHostsActive)
    {
        if (!isClashHostsActive)
        {
            await ProcessKiller.Kill(MainConst.ClashPath);

            return;
        }

        if (ClashConf == null && MessageBox.Show(MainConst._ClashConfInvalidMsg, string.Empty, MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            return;

        MainPres.ClashStatus = ProcessStatus.Initing;

        await ProcessKiller.Kill(MainConst.ClashPath);

        Directory.CreateDirectory(MainConst.AppBasePath);

        if (!File.Exists(GlobalConst.ClashConfPath))
            await File.Create(GlobalConst.ClashConfPath).DisposeAsync();

        ClashConfWatcher.Stop();

        //await File.WriteAllTextAsync(GlobalConst.ClashConfPath, isClashHostsActive switch
        //{
        //    (false, true) => ClashConf!.Doh,
        //    (true, false) => ClashConf!.Hosts,
        //    (true, true) => ClashConf!.Both,
        //    _ => throw new UnreachableException()
        //});

        await Task.Run(() => new ClashProc().Run(Path.GetDirectoryName(MainConst.ClashPath)!, @$"-d ""{Path.GetDirectoryName(GlobalConst.ClashConfPath)}"""));
    }

    private async void BrowserStatusTimer_Tick(object? sender, EventArgs e) => MainPres.BrowserStatus = await StatusManager.GetBrowserStatus(MainPres.BrowserPath);
    private async void NginxStatusTimer_Tick(object? sender, EventArgs e)
    {
        if (MainPres.NginxStatus == ProcessStatus.None)
        {
            NginxConfWatcher.Stop();
            NginxStatusTimer.Stop();
        }
        else
        {
            NginxConfWatcher.Start();
            NginxStatusTimer.Start();
        }

        if (MainPres.NginxStatus == ProcessStatus.Initing)
        {
            try { await Http.GetAsync<HttpResponseMessage>($"https://localhost:{NginxHttpsPort}", MainClient); }
            catch (HttpRequestException ex) when (ex.InnerException is SocketException innerEx && innerEx.SocketErrorCode == SocketError.ConnectionRefused)
            {
                if (StatusManager.GetProxyStatus(MainConst.NginxPath) == ProcessStatus.On)
                    return;

                if (MessageBox.Show(MainConst._NginxLaunchErrorPrompt, string.Empty, MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    Process.Start(new ProcessStartInfo(MainConst.NginxErrorLogPath) { UseShellExecute = true });
            }
            catch { }

            await File.WriteAllTextAsync(GlobalConst.NginxConfPath, NginxExtraConf);
            NginxConfWatcher.Start();
        }

        MainPres.NginxStatus = StatusManager.GetProxyStatus(MainConst.NginxPath);
    }
    private void NginxPortTimer_Tick(object? sender, EventArgs e)
    {
        int nginxHttpPort = MainConst.NginxDefaultHttpPort;
        int nginxHttpsPort = MainConst.NginxDefaultHttpsPort;

        HashSet<int> activeTcpListenersPortSet = [.. IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners().Select(listener => listener.Port)];

        while (activeTcpListenersPortSet.Contains(nginxHttpPort))
            nginxHttpPort++;

        while (activeTcpListenersPortSet.Contains(nginxHttpsPort))
            nginxHttpsPort++;

        if (nginxHttpPort == nginxHttpsPort)
            nginxHttpsPort++;

        if (nginxHttpPort == NginxHttpPort && nginxHttpsPort == NginxHttpsPort)
            return;

        NginxHttpPort = nginxHttpPort;
        NginxHttpsPort = nginxHttpsPort;

        NginxConfWatcher_Changed(null!, null!);
    }
    private async void ClashStatusTimer_Tick(object? sender, EventArgs e)
    {
        if (MainPres.ClashStatus == ProcessStatus.None)
        {
            ClashConfWatcher.Stop();
            ClashStatusTimer.Stop();
        }
        else
        {
            ClashConfWatcher.Start();
            ClashStatusTimer.Start();
        }

        if (MainPres.ClashStatus == ProcessStatus.Initing)
        {
            try { await Http.GetAsync<HttpResponseMessage>($"http://localhost:{ClashMixedPort}", MainClient); }
            catch (HttpRequestException ex) when (ex.InnerException is SocketException innerEx && innerEx.SocketErrorCode == SocketError.ConnectionRefused)
            {
                if (StatusManager.GetProxyStatus(MainConst.ClashPath) == ProcessStatus.On)
                    return;

                MessageBox.Show(MainConst._ClashLaunchErrorMsg);
            }
            catch { }

            await File.WriteAllTextAsync(GlobalConst.ClashConfPath, ClashExtraConf);
            ClashConfWatcher.Start();
        }

        MainPres.ClashStatus = StatusManager.GetProxyStatus(MainConst.ClashPath);
    }
    private void ClashPortTimer_Tick(object? sender, EventArgs e)
    {
        int clashMixedPort = MainConst.ClashDefaultMixedPort;

        HashSet<int> activeTcpListenersPortSet = [.. IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpListeners().Select(listener => listener.Port)];

        while (activeTcpListenersPortSet.Contains(clashMixedPort))
            clashMixedPort++;

        if (clashMixedPort == NginxHttpPort)
            clashMixedPort++;

        if (clashMixedPort == NginxHttpsPort)
            clashMixedPort++;

        if (clashMixedPort == ClashMixedPort)
            return;

        ClashMixedPort = clashMixedPort;

        ClashConfWatcher_Changed(null!, null!);
    }

    private async void CealHostWatcher_Changed(object sender, FileSystemEventArgs e)
    {
        string cealHostName = MainConst.CealHostPrefixRegex().Replace(Path.GetFileNameWithoutExtension(e.Name!), string.Empty);

        try
        {
            CealHostRulesDict[cealHostName] = [];

            if (!File.Exists(e.FullPath))
            {
                CealHostRulesDict.Remove(cealHostName);

                return;
            }

            string cealHost = await File.ReadAllTextAsync(e.FullPath);

            if (cealHost.Length == 0)
                return;

            JsonDocumentOptions cealHostOptions = new() { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip };
            JsonElement cealHostArray = JsonDocument.Parse(cealHost, cealHostOptions).RootElement;

            foreach (JsonElement cealHostRule in cealHostArray.EnumerateArray())
            {
                if (!CealHostRuleValidator.IsValid(cealHostRule))
                {
                    CealHostRulesDict[cealHostName]!.Add(null);

                    continue;
                }

                string cealHostDomains = cealHostRule[0].ToString();
                string? cealHostSni = cealHostRule[1].GetString()?.Trim();
                string cealHostIp = string.IsNullOrWhiteSpace(cealHostRule[2].GetString()) ? "127.0.0.1" : cealHostRule[2].GetString()!.Trim();

                CealHostRulesDict[cealHostName]!.Add(new(cealHostDomains, cealHostSni, cealHostIp));
            }
        }
        catch { CealHostRulesDict[cealHostName] = null; }
        finally
        {
            string hostRules = string.Empty;
            string hostResolverRules = string.Empty;
            int emptySniIndex = 0;

            if (GlobalConst.IsRunningWithAdminPermisson && MainPres.NginxStatus != ProcessStatus.None)
                HostsConf = MainConst.HostsConfStartMarker;

            SubjectAlternativeNameBuilder childCertSanBuilder = new();

            foreach (KeyValuePair<string, List<CealHostRule?>?> cealHostRulesPair in CealHostRulesDict)
                foreach (CealHostRule? cealHostRule in cealHostRulesPair.Value ?? [])
                {
                    if (cealHostRule == null)
                        continue;

                    string[] cealHostDomainArray = JsonSerializer.Deserialize<string[]>(cealHostRule.Domains)!;
                    string cealHostSniWithoutEmpty = string.IsNullOrEmpty(cealHostRule.Sni) ? $"{cealHostRulesPair.Key}{emptySniIndex++}" : cealHostRule.Sni;
                    bool isCealHostDomainArrayContainsValidBrowserDomain = false;

                    foreach (string cealHostDomain in cealHostDomainArray)
                    {
                        if (cealHostDomain.StartsWith('^'))
                            continue;

                        string[] cealHostDomainPair = cealHostDomain.Split('^', 2, StringSplitOptions.TrimEntries);

                        if (!cealHostDomain.StartsWith('$'))
                        {
                            isCealHostDomainArrayContainsValidBrowserDomain = true;

                            hostRules += $"MAP {cealHostDomainPair[0].TrimStart('#')} {cealHostSniWithoutEmpty}," +
                                (!string.IsNullOrEmpty(cealHostDomainPair.ElementAtOrDefault(1)) ? $"EXCLUDE {cealHostDomainPair[1]}," : string.Empty);
                        }

                        if (!GlobalConst.IsRunningWithAdminPermisson || MainPres.NginxStatus == ProcessStatus.None || cealHostDomain.StartsWith('#'))
                            continue;

                        string cealHostIncludeDomain = cealHostDomainPair[0].TrimStart('$');
                        string cealHostIncludeDomainWithoutWildcard = cealHostIncludeDomain.TrimStart('*').TrimStart('.');

                        if (cealHostIncludeDomainWithoutWildcard.Contains('*') || string.IsNullOrEmpty(cealHostIncludeDomainWithoutWildcard))
                            continue;

                        if (cealHostIncludeDomain.StartsWith('*'))
                        {
                            if (MainPres.ClashStatus != ProcessStatus.None)
                                ClashConfHostsDict.TryAdd($"*.{cealHostIncludeDomainWithoutWildcard}", "127.0.0.1");

                            HostsConf += $"127.0.0.1 www.{cealHostIncludeDomainWithoutWildcard}{Environment.NewLine}";

                            childCertSanBuilder.AddDnsName($"*.{cealHostIncludeDomainWithoutWildcard}");

                            if (cealHostIncludeDomain.StartsWith("*."))
                                continue;
                        }

                        if (MainPres.ClashStatus != ProcessStatus.None)
                            ClashConfHostsDict.TryAdd(cealHostIncludeDomainWithoutWildcard, "127.0.0.1");

                        HostsConf += $"127.0.0.1 {cealHostIncludeDomainWithoutWildcard}{Environment.NewLine}";

                        childCertSanBuilder.AddDnsName(cealHostIncludeDomainWithoutWildcard);
                    }

                    if (!isCealHostDomainArrayContainsValidBrowserDomain)
                        continue;

                    hostResolverRules += $"MAP {cealHostSniWithoutEmpty} {cealHostRule.Ip},";
                }

            CealArgs = @$"--host-rules=""{hostRules.TrimEnd(',')}"" --host-resolver-rules=""{hostResolverRules.TrimEnd(',')}"" --test-type --ignore-certificate-errors";

            if (GlobalConst.IsRunningWithAdminPermisson && MainPres.NginxStatus != ProcessStatus.None)
            {
                HostsConf += MainConst.HostsConfEndMarker;

                RSA certKey = RSA.Create(2048);

                CertificateRequest rootCertRequest = new(MainConst.RootCertSubjectName, certKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                rootCertRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, false));
                RootCert = rootCertRequest.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddYears(100));

                CertificateRequest childCertRequest = new(MainConst.ChildCertSubjectName, certKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                childCertRequest.CertificateExtensions.Add(childCertSanBuilder.Build());
                ChildCert = childCertRequest.Create(RootCert, RootCert.NotBefore, RootCert.NotAfter, Guid.NewGuid().ToByteArray());

                CertKey = certKey;

                NginxConfWatcher_Changed(null!, null!);

                if (MainPres.ClashStatus != ProcessStatus.None)
                    ClashConfWatcher_Changed(null!, null!);
            }
        }
    }
    private void CealHostWatcher_Renamed(object sender, RenamedEventArgs e)
    {
        CealHostWatcher_Changed(null!, new(new(), Path.GetDirectoryName(e.OldFullPath)!, e.OldName));
        CealHostWatcher_Changed(null!, new(new(), Path.GetDirectoryName(e.FullPath)!, e.Name));
    }
    private async void NginxConfWatcher_Changed(object sender, FileSystemEventArgs e)
    {
        try
        {
            NginxConfig nginxExtraConf = NginxConfig.Load(NginxExtraConf = await File.ReadAllTextAsync(GlobalConst.NginxConfPath));
            int nginxServerIndex = 0;

            foreach (IToken mainToken in nginxExtraConf.GetTokens())
                if (mainToken is GroupToken mainGroupToken && mainGroupToken.Key.Equals("http", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (IToken httpToken in mainGroupToken.Tokens)
                        if (httpToken is GroupToken httpGroupToken && httpGroupToken.Key.Equals("server", StringComparison.OrdinalIgnoreCase))
                            nginxServerIndex++;

                    break;
                }

            NginxConf = nginxExtraConf
                .AddOrUpdate("worker_processes", "auto")
                .AddOrUpdate("events:worker_connections", "65536")
                .AddOrUpdate("http:proxy_set_header", "Host $http_host")
                .AddOrUpdate("http:proxy_ssl_server_name", "on")
                .AddOrUpdate("http:proxy_buffer_size", "14k")
                .AddOrUpdate($"http:server[{nginxServerIndex}]:listen", $"{NginxHttpPort} default_server")
                .AddOrUpdate($"http:server[{nginxServerIndex}]:return", "https://$host$request_uri");

            int emptySniIndex = 0;

            foreach (KeyValuePair<string, List<CealHostRule?>?> cealHostRulesPair in CealHostRulesDict)
                foreach (CealHostRule? cealHostRule in cealHostRulesPair.Value ?? [])
                {
                    if (cealHostRule == null)
                        continue;

                    string[] cealHostDomainArray = JsonSerializer.Deserialize<string[]>(cealHostRule.Domains)!;
                    string cealHostSniWithoutEmpty = string.IsNullOrEmpty(cealHostRule.Sni) ? $"{cealHostRulesPair.Key}{emptySniIndex++}" : cealHostRule.Sni;
                    string nginxServerName = "~";

                    foreach (string cealHostDomain in cealHostDomainArray)
                    {
                        if (cealHostDomain.StartsWith('#') || cealHostDomain.StartsWith('^'))
                            continue;

                        string[] cealHostDomainPair = cealHostDomain.Split('^', 2, StringSplitOptions.TrimEntries);

                        nginxServerName += "^" + (!string.IsNullOrEmpty(cealHostDomainPair.ElementAtOrDefault(1)) ? $"(?!{cealHostDomainPair[1].Replace(".", "\\.").Replace("*", ".*")})" : string.Empty) +
                            cealHostDomainPair[0].TrimStart('$').Replace(".", "\\.").Replace("*", ".*") + "$|";
                    }

                    if (nginxServerName == "~")
                        continue;

                    nginxServerIndex++;

                    NginxConf = NginxConf
                        .AddOrUpdate($"http:server[{nginxServerIndex}]:server_name", nginxServerName.TrimEnd('|'))
                        .AddOrUpdate($"http:server[{nginxServerIndex}]:listen", $"{NginxHttpsPort} ssl")
                        .AddOrUpdate($"http:server[{nginxServerIndex}]:ssl_certificate", Path.GetFileName(MainConst.NginxCertPath))
                        .AddOrUpdate($"http:server[{nginxServerIndex}]:ssl_certificate_key", Path.GetFileName(MainConst.NginxKeyPath))
                        .AddOrUpdate($"http:server[{nginxServerIndex}]:location", "/", true)
                        .AddOrUpdate($"http:server[{nginxServerIndex}]:location:proxy_pass", $"https://{cealHostRule.Ip}");

                    NginxConf = cealHostRule.Sni == null ?
                        NginxConf.AddOrUpdate($"http:server[{nginxServerIndex}]:proxy_ssl_server_name", "off") :
                        NginxConf.AddOrUpdate($"http:server[{nginxServerIndex}]:proxy_ssl_name", cealHostSniWithoutEmpty);
                }
        }
        catch { NginxConf = null; }
    }
    private void NginxConfWatcher_Renamed(object sender, RenamedEventArgs e)
    {
        NginxConfWatcher_Changed(null!, new(new(), Path.GetDirectoryName(e.OldFullPath)!, e.OldName));
        NginxConfWatcher_Changed(null!, new(new(), Path.GetDirectoryName(e.FullPath)!, e.Name));
    }
    private async void ClashConfWatcher_Changed(object sender, FileSystemEventArgs e)
    {
        try
        {
            ISerializer clashConfSerializer = new SerializerBuilder().WithNamingConvention(HyphenatedNamingConvention.Instance).Build();

            Dictionary<string, object> clashDohConfDict = new
            (
                new DeserializerBuilder()
                .WithNamingConvention(HyphenatedNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build()
                .Deserialize<Dictionary<string, object>>(ClashExtraConf = await File.ReadAllTextAsync(GlobalConst.ClashConfPath))
            )
            {
                ["mixed-port"] = ClashMixedPort,
                ["tun"] = new
                {
                    enable = true,
                    stack = "system",
                    autoRoute = true,
                    autoDetectInterface = true,
                    dnsHijack = new[] { "any:53", "tcp://any:53" }
                },
                ["dns"] = new
                {
                    enable = true,
                    listen = ":53",
                    ipv6 = true,
                    nameserver = MainConst.ClashNameServers
                }
            };

            ClashConf = new()
            {
                Doh = clashConfSerializer.Serialize(clashDohConfDict),
                Hosts = clashConfSerializer.Serialize(new Dictionary<string, object>(clashDohConfDict)
                {
                    ["dns"] = new
                    {
                        enable = true,
                        listen = ":53",
                        ipv6 = true
                    },
                    ["hosts"] = ClashConfHostsDict
                }),
                Both = clashConfSerializer.Serialize(new Dictionary<string, object>(clashDohConfDict) { ["hosts"] = ClashConfHostsDict })
            };
        }
        catch { ClashConf = null; }
    }
    private void ClashConfWatcher_Renamed(object sender, RenamedEventArgs e)
    {
        ClashConfWatcher_Changed(null!, new(new(), Path.GetDirectoryName(e.OldFullPath)!, e.OldName));
        ClashConfWatcher_Changed(null!, new(new(), Path.GetDirectoryName(e.FullPath)!, e.Name));
    }
    private void FileSystemWatcher_Error(object sender, ErrorEventArgs e) => throw e.GetException();

    private void MainWin_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyboardDevice.Modifiers != ModifierKeys.Control)
            return;

        if (e.Key == System.Windows.Input.Key.W)
            Application.Current.Shutdown();
        else if (e.Key == System.Windows.Input.Key.H)
        {
            System.Windows.Forms.NotifyIcon notifyIcon = new() { Icon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location), Text = MainConst.DefaultWindowTitle, Visible = true };

            notifyIcon.Click += (_, _) =>
            {
                Show();

                notifyIcon.Dispose();
            };

            Hide();
        }
    }

    private void GithubButton_Click(object sender, RoutedEventArgs e) => Process.Start(new ProcessStartInfo(MainConst.GithubRepoUrl) { UseShellExecute = true });
    private void UpdateHostButton_Click(object? sender, RoutedEventArgs e)
    {
        
    }
    private void UpdateSoftwareButton_Click(object? sender, RoutedEventArgs e)
    {

    }

    private void FlyoutListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => MainPres.IsFlyoutOpen = false;

    private void LeftDialogButton_Click(object sender, RoutedEventArgs e) => DialogHost.Close(null, Models.DialogResult.Left);
    private void RightDialogButton_Click(object sender, RoutedEventArgs e) => DialogHost.Close(null, Models.DialogResult.Right);

    private void PageFrame_Navigated(object sender, NavigationEventArgs e) => MainPres.WindowTitle = $"{((Page)e.Content).Title} - {MainConst.DefaultWindowTitle}";

    private void ElevateButton_Click(object sender, RoutedEventArgs e) => AdminPermissionElevator.RestartWithAdminPermission();
}