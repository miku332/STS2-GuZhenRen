using System.Reflection;
using GuZhenRen.Relics;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using STS2RitsuLib.Patching.Models;

namespace GuZhenRen.Patches;

public sealed class CunGuangYinSmithCountPatch : IPatchMethod
{
    private static readonly PropertyInfo OwnerProperty =
        typeof(RestSiteOption).GetProperty(
            "Owner",
            BindingFlags.Instance | BindingFlags.NonPublic)!;

    public static string PatchId => "cun-guang-yin-smith-count";

    public static string Description => "Cun Guang Yin makes rest-site smithing upgrade two cards.";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(
            typeof(SmithRestSiteOption),
            nameof(SmithRestSiteOption.SmithCount),
            Type.EmptyTypes,
            MethodType.Getter)
    ];

    public static bool Prefix(SmithRestSiteOption __instance, ref int __result)
    {
        if (OwnerProperty.GetValue(__instance) is not Player player
            || player.GetRelic<CunGuangYin>() is null)
        {
            return true;
        }

        __result = 2;
        return false;
    }
}
