namespace Sheas_Cealer.Models;

internal class DialogContent
{
    internal DialogContent(string title, string? message, DialogButton? leftButton, DialogButton rightButton)
    {
        Title = title;
        Message = message;
        LeftButton = leftButton;
        RightButton = rightButton;
    }

    public string Title { get; init; } = string.Empty;
    public string? Message { get; init; } = null;
    public DialogButton? LeftButton { get; init; } = null;
    public DialogButton RightButton { get; init; } = null!;
}