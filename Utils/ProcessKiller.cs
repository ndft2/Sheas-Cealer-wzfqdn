using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Sheas_Cealer.Utils;

internal static class ProcessKiller
{
    internal static async Task Kill(string processPath)
    {
        List<Task> processWaitForExitTaskList = [];

        foreach (Process process in Process.GetProcessesByName(Path.GetFileNameWithoutExtension(processPath)))
        {
            process.Kill();

            processWaitForExitTaskList.Add(process.WaitForExitAsync());
        }

        await Task.WhenAll(processWaitForExitTaskList);
    }
}