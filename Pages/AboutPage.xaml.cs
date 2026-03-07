using Sheas_Cealer.Consts;
using Sheas_Cealer.Models;
using Sheas_Cealer.Preses;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Sheas_Cealer.Pages;

internal partial class AboutPage : Page
{
    private readonly AboutPres AboutPres;

    internal AboutPage()
    {
        InitializeComponent();

        AboutPres = new();
    }

    private async void CopyLinkButton_Clicked(object sender, RoutedEventArgs e)
    {
        Button senderButton = (Button)sender;

        Clipboard.SetText(((AboutInfo)senderButton.DataContext).Url);

        AboutPres.SnackbarMessageQueue.Enqueue(AboutConst._LinkCopiedSnackbarMsg);
    }
    private async void GotoLinkButton_Clicked(object sender, RoutedEventArgs e)
    {
        Button senderButton = (Button)sender;

        Process.Start(new ProcessStartInfo(((AboutInfo)senderButton.DataContext).Url!) { UseShellExecute = true });
    }
}