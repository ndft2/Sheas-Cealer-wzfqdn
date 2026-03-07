using CommunityToolkit.Mvvm.ComponentModel;

namespace Sheas_Cealer.Preses;

internal partial class GamePres : GlobalPres
{
    [ObservableProperty]
    private bool isGameRunning = false;
}