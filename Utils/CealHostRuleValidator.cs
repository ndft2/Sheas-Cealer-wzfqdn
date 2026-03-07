using System.Linq;
using System.Text.Json;

namespace Sheas_Cealer.Utils;

internal static class CealHostRuleValidator
{
    internal static bool IsValid(JsonElement cealHostRule)
    {
        if (cealHostRule.ValueKind != JsonValueKind.Array ||
            cealHostRule.EnumerateArray().Count() != 3 ||
            cealHostRule[0].ValueKind != JsonValueKind.Array ||
            !cealHostRule[0].EnumerateArray().Any() ||
            cealHostRule[1].ValueKind != JsonValueKind.String &&
            cealHostRule[1].ValueKind != JsonValueKind.Null ||
            cealHostRule[2].ValueKind != JsonValueKind.String)
            return false;

        foreach (JsonElement customHostDomain in cealHostRule[0].EnumerateArray())
            if (customHostDomain.ValueKind != JsonValueKind.String || string.IsNullOrEmpty(customHostDomain.GetString()?.Trim().TrimStart("#$^".ToCharArray())))
                return false;

        return true;
    }
}