using Ona_Core;
using Sheas_Cealer.Consts;
using Sheas_Cealer.Preses;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Sheas_Cealer.Pages;

internal partial class RulesPage : Page
{
    private readonly RulesPres RulesPres;

    private readonly HttpClient RulesClient = new(new HttpClientHandler { ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator });

    internal RulesPage() => InitializeComponent();

    private async void EditHostButton_Click(object sender, RoutedEventArgs e)
    {
        Button senderButton = (Button)sender;
        string cealHostPath = senderButton == EditLocalHostButton ? MainConst.LocalHostPath : MainConst.UpstreamHostPath;

        if (!File.Exists(cealHostPath))
            await File.Create(cealHostPath).DisposeAsync();

        try { Process.Start(new ProcessStartInfo(cealHostPath) { UseShellExecute = true }); }
        catch (UnauthorizedAccessException) { Process.Start(new ProcessStartInfo(cealHostPath) { UseShellExecute = true, Verb = "RunAs" }); }
    }

    private async void UpdateUpstreamHostButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (!File.Exists(MainConst.UpstreamHostPath))
                await File.Create(MainConst.UpstreamHostPath).DisposeAsync();

            string upstreamUpstreamHostUrl = (RulesPres.UpstreamUrl.StartsWith("http://") || RulesPres.UpstreamUrl.StartsWith("https://") ? string.Empty : "https://") + RulesPres.UpstreamUrl;
            string upstreamUpstreamHostString = await Http.GetAsync<string>(upstreamUpstreamHostUrl, RulesClient);
            string localUpstreamHostString = await File.ReadAllTextAsync(MainConst.UpstreamHostPath);

            try { upstreamUpstreamHostString = Encoding.UTF8.GetString(Convert.FromBase64String(upstreamUpstreamHostString)); }
            catch { }

            if (sender == null && (localUpstreamHostString != upstreamUpstreamHostString && localUpstreamHostString.ReplaceLineEndings() != upstreamUpstreamHostString.ReplaceLineEndings()))
                RulesPres.IsUpstreamHostUtd = false;
            else if (sender != null)
                if (localUpstreamHostString == upstreamUpstreamHostString || localUpstreamHostString.ReplaceLineEndings() == upstreamUpstreamHostString.ReplaceLineEndings())
                {
                    RulesPres.IsUpstreamHostUtd = true;

                    MessageBox.Show(MainConst._UpstreamHostUtdMsg);
                }
                else
                {
                    MessageBoxResult overrideOptionResult = MessageBox.Show(MainConst._OverrideUpstreamHostPrompt, string.Empty, MessageBoxButton.YesNoCancel);

                    if (overrideOptionResult == MessageBoxResult.Yes)
                    {
                        await File.WriteAllTextAsync(MainConst.UpstreamHostPath, upstreamUpstreamHostString);

                        RulesPres.IsUpstreamHostUtd = true;

                        MessageBox.Show(MainConst._UpdateUpstreamHostSuccessMsg);
                    }
                    else if (overrideOptionResult == MessageBoxResult.No)
                        try { Process.Start(new ProcessStartInfo(upstreamUpstreamHostUrl) { UseShellExecute = true }); }
                        catch (UnauthorizedAccessException) { Process.Start(new ProcessStartInfo(upstreamUpstreamHostUrl) { UseShellExecute = true, Verb = "RunAs" }); }
                }
        }
        catch when (sender == null) { }
    }
}
