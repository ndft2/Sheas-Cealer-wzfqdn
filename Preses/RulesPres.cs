using CommunityToolkit.Mvvm.ComponentModel;

namespace Sheas_Cealer.Preses;

internal partial class RulesPres : GlobalPres
{
    [ObservableProperty]
    private bool isUpstreamHostUtd = true;
}