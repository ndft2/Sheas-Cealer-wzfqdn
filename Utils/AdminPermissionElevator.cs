using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace Sheas_Cealer.Utils;

internal static class AdminPermissionElevator
{
    internal static void RestartWithAdminPermission()
    {
        try
        {
            Process.Start(new ProcessStartInfo(Process.GetCurrentProcess().MainModule?.FileName ?? Environment.GetCommandLineArgs().First()) { UseShellExecute = true, Verb = "RunAs" });

            Application.Current.Shutdown();
        }
        catch (Win32Exception ex) when (ex.NativeErrorCode == 1223) { }
    }
}