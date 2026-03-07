using Sheas_Core;
using System;
using System.Windows;

namespace Sheas_Cealer.Proces;

internal class BrowserProc : Proc
{
    private readonly bool IsShutDownRequested;

    internal BrowserProc(string browserPath, bool isShutDownRequested) : base(browserPath) => IsShutDownRequested = isShutDownRequested;

    protected sealed override void Process_Exited(object? sender, EventArgs e)
    {
        if (IsShutDownRequested)
            Application.Current.Dispatcher.InvokeShutdown();
    }
}