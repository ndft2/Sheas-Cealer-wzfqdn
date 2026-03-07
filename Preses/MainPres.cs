using CommunityToolkit.Mvvm.ComponentModel;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Sheas_Cealer.Consts;
using Sheas_Cealer.Models;
using Sheas_Cealer.Props;
using Sheas_Cealer.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Windows.Controls;
using File = System.IO.File;

namespace Sheas_Cealer.Preses;

internal partial class MainPres : GlobalPres
{
    internal MainPres(FlyoutItem initialFlyoutItem)
    {
        CurrentFlyoutItem = initialFlyoutItem;

        if (!GlobalConst.IsRunningWithAdminPermisson)
            return;

        NginxStatus = StatusManager.GetProxyStatus(MainConst.NginxPath);
        ClashStatus = StatusManager.GetProxyStatus(MainConst.ClashPath);

        if (IsSpeedMonitorEnabled)
        {
            DownloadSpeed = MainConst.DefaultOnDownloadSpeed;
            UploadSpeed = MainConst.DefaultOnUploadSpeed;
        }
    }

    [ObservableProperty]
    private int[] topBarArray = [0, 1, 2];

    [ObservableProperty]
    private string windowTitle = MainConst.DefaultWindowTitle;

    [ObservableProperty]
    private string downloadSpeed = MainConst.DefaultOffDownloadSpeed;

    [ObservableProperty]
    private string uploadSpeed = MainConst.DefaultOffUploadSpeed;

    [ObservableProperty]
    private bool isPageLoading = true;

    [ObservableProperty]
    private string browserStatusMessage = string.Empty;

    [ObservableProperty]
    private string nginxStatusMessage = string.Empty;

    [ObservableProperty]
    private string clashStatusMessage = string.Empty;

    [ObservableProperty]
    private double statusProgress = 50;

    [ObservableProperty]
    private bool isHostCollectionAtBottom = false;

    [ObservableProperty]
    private bool isFlyoutOpen = false;

    [ObservableProperty]
    private bool isExpandBarOpen = false;
    partial void OnIsExpandBarOpenChanged(bool value)
    {
        Settings.Default.IsExpandBarOpen = value;
        Settings.Default.Save();
    }

    [ObservableProperty]
    private FlyoutItem currentFlyoutItem;
}