using Sheas_Cealer.Consts;
using Sheas_Cealer.Models;
using Sheas_Cealer.Preses;
using Sheas_Cealer.Utils;
using Sheas_Cealer.Valids;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Sheas_Cealer.Pages;

internal partial class SettingsPage : Page
{
    private readonly SettingsPres SettingsPres;

    internal SettingsPage()
    {
        InitializeComponent();

        DataContext = SettingsPres = new();
    }

    private void BrowserPathComboBox_PreviewDragHover(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) &&
            e.Data.GetData(DataFormats.FileDrop) is string[] filePathArray &&
            new SettingsBrowserPathTextBoxTextValid().Validate(filePathArray[0], CultureInfo.CurrentCulture).IsValid ?
            DragDropEffects.Link : DragDropEffects.None;

        e.Handled = true;
    }
    private void BrowserPathComboBox_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop) &&
            e.Data.GetData(DataFormats.FileDrop) is string[] filePathArray &&
            new SettingsBrowserPathTextBoxTextValid().Validate(filePathArray[0], CultureInfo.CurrentCulture).IsValid)
            SettingsPres.BrowserPath = filePathArray[0];
    }

    private async void NginxConfButton_Click(object sender, RoutedEventArgs e)
    {
        if (!GlobalConst.IsRunningWithAdminPermisson)
        {
            SettingsPres.SnackbarMessageQueue.Enqueue(GlobalConst._AdminPermissionRequiredSnackbarMsg, GlobalConst._AdminPermissionRequiredSnackbarButtonContent, _ => AdminPermissionElevator.RestartWithAdminPermission(), null, true, false, TimeSpan.FromSeconds(3.5));

            return;
        }

        if (SettingsPres.NginxStatus == ProcessStatus.None)
        {
            SettingsPres.SnackbarMessageQueue.Enqueue(GlobalConst._NginxPluginRequiredSnackbarMsg, GlobalConst._PluginRequiredSnackbarButtonContent, _ => Process.Start(new ProcessStartInfo(GlobalConst.GithubReleaseUrl) { UseShellExecute = true }), null, true, false, TimeSpan.FromSeconds(3.5));

            return;
        }
        else if (SettingsPres.NginxStatus == ProcessStatus.Initing)
        {
            SettingsPres.SnackbarMessageQueue.Enqueue(GlobalConst._NginxWaitingForInitingSnackbarMsg, null, null, null, true, false, TimeSpan.FromSeconds(3.5));

            return;
        }

        if (!File.Exists(GlobalConst.NginxConfPath))
            await File.Create(GlobalConst.NginxConfPath).DisposeAsync();

        Process.Start(new ProcessStartInfo(GlobalConst.NginxConfPath) { UseShellExecute = true });
    }
    private async void ClashConfButton_Click(object sender, RoutedEventArgs e)
    {
        if (!GlobalConst.IsRunningWithAdminPermisson)
        {
            SettingsPres.SnackbarMessageQueue.Enqueue(GlobalConst._AdminPermissionRequiredSnackbarMsg, GlobalConst._AdminPermissionRequiredSnackbarButtonContent, _ => AdminPermissionElevator.RestartWithAdminPermission(), null, true, false, TimeSpan.FromSeconds(3.5));

            return;
        }

        if (SettingsPres.ClashStatus == ProcessStatus.None)
        {
            SettingsPres.SnackbarMessageQueue.Enqueue(GlobalConst._ClashPluginRequiredSnackbarMsg, GlobalConst._PluginRequiredSnackbarButtonContent, _ => Process.Start(new ProcessStartInfo(GlobalConst.GithubReleaseUrl) { UseShellExecute = true }), null, true, false, TimeSpan.FromSeconds(3.5));

            return;
        }
        else if (SettingsPres.ClashStatus == ProcessStatus.Initing)
        {
            SettingsPres.SnackbarMessageQueue.Enqueue(GlobalConst._ClashWaitingForInitingSnackbarMsg, null, null, null, true, false, TimeSpan.FromSeconds(3.5));

            return;
        }

        if (!File.Exists(GlobalConst.ClashConfPath))
            await File.Create(GlobalConst.ClashConfPath).DisposeAsync();

        Process.Start(new ProcessStartInfo(GlobalConst.ClashConfPath) { UseShellExecute = true });
    }
    private async void HostsConfButton_Click(object sender, RoutedEventArgs e)
    {
        if (!GlobalConst.IsRunningWithAdminPermisson)
        {
            SettingsPres.SnackbarMessageQueue.Enqueue(GlobalConst._AdminPermissionRequiredSnackbarMsg, GlobalConst._AdminPermissionRequiredSnackbarButtonContent, _ => AdminPermissionElevator.RestartWithAdminPermission(), null, true, false, TimeSpan.FromSeconds(3.5));

            return;
        }

        if (!File.Exists(GlobalConst.HostsConfPath))
            await File.Create(GlobalConst.HostsConfPath).DisposeAsync();

        File.SetAttributes(GlobalConst.HostsConfPath, File.GetAttributes(GlobalConst.HostsConfPath) & ~FileAttributes.ReadOnly);

        Process.Start(new ProcessStartInfo(GlobalConst.HostsConfPath) { UseShellExecute = true });
    }
}