using GuZhenRen.Potions;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Patching.Models;

namespace GuZhenRen.Patches;

public sealed class FuRenXinPotionRemovalPatch : IPatchMethod
{
    public static string PatchId => "fu-ren-xin-clear-slot";

    public static string Description =>
        "Clears Fu Ren Xin growth when its potion slot is emptied.";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(
            typeof(Player),
            "RemovePotionInternal",
            [typeof(PotionModel)])
    ];

    public static void Prefix(Player __instance, PotionModel potion)
    {
        FuRenXin.ClearSlotBeforeRemoval(__instance, potion);
    }
}
