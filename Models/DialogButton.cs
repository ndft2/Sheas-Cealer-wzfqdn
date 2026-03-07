namespace Sheas_Cealer.Models;

internal class DialogButton
{
    internal DialogButton(string content, bool isDefault = false, bool isCancel = false)
    {
        Content = content;
        IsDefault = isDefault;
        IsCancel = isCancel;
    }

    public string Content { get; init; } = string.Empty;
    public bool IsDefault { get; init; } = false;
    public bool IsCancel { get; init; } = false;
}