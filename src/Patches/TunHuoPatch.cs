using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Patching.Models;
using HarmonyLib;
using GuZhenRen.Powers;

namespace GuZhenRen.Patches;

public sealed class TunHuoPatch : IPatchMethod
{
    public static string PatchId => "tun_huo_skip_burn_damage";

    public static string Description => "吞火 prevents Burn from damaging its owner";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(typeof(Burn), "OnTurnEndInHand")
    ];

    public static bool Prefix(Burn __instance, ref Task __result)
    {
        if (__instance.Owner.Creature.GetPower<TunHuoPower>() is null)
        {
            return true;
        }

        __result = Task.CompletedTask;
        return false;
    }
}
