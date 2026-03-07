using System.Text.RegularExpressions;

namespace Sheas_Cealer.Consts;

internal abstract partial class SettingsConst : SettingsMultilangConst
{
    public static string[] ThemeColorNameArray => [.. GlobalConst.ThemeColorDictionary.Keys];
    public static string[] ThemeStateNameArray => [.. GlobalConst.ThemeStateDictionary.Keys];
    public static string[] LangOptionNameArray => [.. GlobalConst.LangOptionDictionary.Keys];

    [GeneratedRegex("^(https?:\\/\\/)?[a-zA-Z0-9](-*[a-zA-Z0-9])*(\\.[a-zA-Z0-9](-*[a-zA-Z0-9])*)*(:\\d{1,5})?(\\/[a-zA-Z0-9.\\-_\\~\\!\\$\\&\\'\\(\\)\\*\\+\\,\\;\\=\\:\\@\\%]*)*$")]
    internal static partial Regex UpstreamUrlRegex();

    [GeneratedRegex("^(--[a-z](-?[a-z])*(=((\".*\")|\\S+))?( --[a-z](-?[a-z])*(=((\".*\")|\\S+))?)*)?$")]
    internal static partial Regex ExtraArgsRegex();
}