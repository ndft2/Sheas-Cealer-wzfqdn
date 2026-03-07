using Sheas_Cealer.Consts;
using Sheas_Cealer.Models;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading.Tasks;

namespace Sheas_Cealer.Utils;

internal static class StatusManager
{
    internal static ProcessStatus GetProxyStatus(string proxyPath) => File.Exists(proxyPath) ?
        Process.GetProcessesByName(Path.GetFileNameWithoutExtension(proxyPath)).Length != 0 ?
        ProcessStatus.On : ProcessStatus.Off : ProcessStatus.None;

    internal static async Task<ProcessStatus> GetBrowserStatus(string browserPath) => await Task.Run(() => File.Exists(browserPath) ?
        Process.GetProcessesByName(Path.GetFileNameWithoutExtension(browserPath)).Length != 0 &&
        new ManagementObjectSearcher($"SELECT CommandLine FROM Win32_Process WHERE Name = '{Path.GetFileName(browserPath)}'").Get().Cast<ManagementObject>().FirstOrDefault()?["CommandLine"]?.ToString() is string browserCommandLine &&
        browserCommandLine.Contains("--host-rules=") && browserCommandLine.Contains("--host-resolver-rules=") ?
        ProcessStatus.On : ProcessStatus.Off : ProcessStatus.None);
}