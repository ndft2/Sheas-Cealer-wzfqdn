using Sheas_Cealer.Consts;
using System.Collections;
using System.Globalization;

namespace Sheas_Cealer.Utils;

internal static class ResourceKeyFinder
{
    internal static string? FindGlobalKey(string value)
    {
        foreach (DictionaryEntry globalEntry in GlobalConst.ResourceManager.GetResourceSet(CultureInfo.CurrentCulture, false, false)!)
            if (globalEntry.Value!.ToString() == value)
                return globalEntry.Key.ToString();

        return null;
    }
}