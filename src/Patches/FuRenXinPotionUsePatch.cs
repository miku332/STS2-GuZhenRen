using GuZhenRen.Potions;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Patching.Models;

namespace GuZhenRen.Patches;

public sealed class FuRenXinPotionUsePatch : IPatchMethod
{
    public static string PatchId => "fu-ren-xin-sync-before-use";

    public static string Description =>
        "Synchronizes Fu Ren Xin's accumulated poison before it is used.";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(
            typeof(PotionModel),
            nameof(PotionModel.EnqueueManualUse),
            [typeof(Creature)])
    ];

    public static bool Prefix(PotionModel __instance)
    {
        return __instance is not FuRenXin potion
            || FuRenXin.PrepareManualUse(potion);
    }
}
