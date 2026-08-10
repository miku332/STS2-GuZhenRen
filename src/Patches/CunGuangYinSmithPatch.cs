using System.Reflection;
using GuZhenRen.Relics;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using STS2RitsuLib.Patching.Models;

namespace GuZhenRen.Patches;

public sealed class CunGuangYinSmithPatch : IPatchMethod
{
    private static readonly PropertyInfo OwnerProperty =
        typeof(RestSiteOption).GetProperty(
            "Owner",
            BindingFlags.Instance | BindingFlags.NonPublic)!;

    public static string PatchId => "cun-guang-yin-extra-smith";

    public static string Description => "Cun Guang Yin upgrades one additional card at rest sites.";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(
            typeof(SmithRestSiteOption),
            ".ctor",
            [typeof(Player)],
            MethodType.Constructor),
        new ModPatchTarget(
            typeof(SmithRestSiteOption),
            nameof(SmithRestSiteOption.OnSelect),
            Type.EmptyTypes)
    ];

    public static void Postfix(SmithRestSiteOption __instance, Player player)
    {
        if (player.GetRelic<CunGuangYin>() is null)
        {
            return;
        }

        __instance.SmithCount = 2;
    }

    public static void Prefix(SmithRestSiteOption __instance)
    {
        if (OwnerProperty.GetValue(__instance) is not Player player
            || player.GetRelic<CunGuangYin>() is null)
        {
            return;
        }

        __instance.SmithCount = 2;
    }
}
