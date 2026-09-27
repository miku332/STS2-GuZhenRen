using HarmonyLib;
using GuZhenRen.Systems;
using MegaCrit.Sts2.Core.Localization;

namespace GuZhenRen.Patches;

[HarmonyPatch(typeof(LocString), nameof(LocString.GetRawText))]
public static class GuTermReplacementPatch
{
    [HarmonyPostfix]
    public static void Postfix(LocString __instance, ref string __result)
    {
        if (!__instance.LocEntryKey.StartsWith(
                "GU_ZHEN_REN_",
                StringComparison.Ordinal)
            || !__result.Contains('蛊')
            || !string.Equals(
                LocManager.Instance?.Language,
                "zhs",
                StringComparison.Ordinal)
            || !GuZhenRenSettingsPage.ReplaceGuWithQu)
        {
            return;
        }

        __result = __result.Replace('蛊', '蛆');
    }
}
