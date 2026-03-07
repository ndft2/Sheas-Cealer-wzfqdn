using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Sheas_Cealer.Consts;

internal abstract partial class MainConst : MainMultilangConst
{
    internal static string[] TrafficSpeedUnitsArray => ["B/s", "KB/s", "MB/s", "GB/s", "TB/s", "PB/s", "EB/s"];
    internal static string DefaultOffDownloadSpeed => $"↓ -- {TrafficSpeedUnitsArray[0]}";
    internal static string DefaultOffUploadSpeed => $"↑ -- {TrafficSpeedUnitsArray[0]}";
    internal static string DefaultOnDownloadSpeed => $"↓ 0 {TrafficSpeedUnitsArray[0]}";
    internal static string DefaultOnUploadSpeed => $"↑ 0 {TrafficSpeedUnitsArray[0]}";

    internal static string AppBasePath => AppDomain.CurrentDomain.SetupInformation.ApplicationBase!;
    internal static string CealHostPath => Path.Combine(AppBasePath, "Cealing-Host-*.json");
    internal static string UpstreamHostPath => Path.Combine(AppBasePath, "Cealing-Host-U.json");
    internal static string LocalHostPath => Path.Combine(AppBasePath, "Cealing-Host-L.json");
    internal static string GithubRepoUrl => "https://github.com/SpaceTimee/Sheas-Cealer";
    internal static string GithubReleaseUrl => "https://github.com/SpaceTimee/Sheas-Cealer/releases/latest";
    internal static string GithubMirrorUrl => "https://ghfast.top/";
    internal static string UpdateApiUrl => "https://api.github.com/repos/SpaceTimee/Sheas-Cealer/releases/latest";
    internal static string UpdateApiUserAgent => "Sheas-Cealer";
    internal static string DisableCealArg => "--disable-cealing";

    internal static string HostsConfStartMarker => $"# Cealing Nginx Start{Environment.NewLine}";
    internal static string HostsConfEndMarker => "# Cealing Nginx End";

    internal static string NginxPath => Path.Combine(AppBasePath, "Cealing-Nginx.exe");
    internal static string NginxLogsPath => Path.Combine(AppBasePath, "logs");
    internal static string NginxErrorLogPath => Path.Combine(NginxLogsPath, "error.log");
    internal static string NginxTempPath => Path.Combine(AppBasePath, "temp");
    internal static string NginxCertPath => Path.Combine(AppBasePath, "Cealing-Cert.pem");
    internal static string NginxKeyPath => Path.Combine(AppBasePath, "Cealing-Key.pem");
    internal static int NginxDefaultHttpPort => 80;
    internal static int NginxDefaultHttpsPort => 443;
    internal static string RootCertSubjectName => "CN=Cealing Cert Root";
    internal static string ChildCertSubjectName => "CN=Cealing Cert Child";

    internal static string ClashPath => Path.Combine(AppBasePath, "Cealing-Clash.exe");
    internal static string[] ClashNameServers => ["https://ns.net.kg/dns-query", "https://dnschina1.soraharu.com/dns-query", "https://0ms.dev/dns-query"];
    internal static int ClashDefaultMixedPort => 7880;

    internal static string DefaultWindowTitle => "Sheas Cealer";

    [GeneratedRegex("^Cealing-Host-")]
    internal static partial Regex CealHostPrefixRegex();
}