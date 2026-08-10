using GuZhenRen.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Patching.Models;

namespace GuZhenRen.Patches;

public sealed class JianChiGuDecrementPatch : IPatchMethod
{
    public static string PatchId => "jian-chi-gu-decrement";

    public static string Description =>
        "Jian Chi Gu covers buffs that expire through PowerCmd.Decrement.";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(
            typeof(PowerCmd),
            nameof(PowerCmd.Decrement),
            [typeof(PowerModel)])
    ];

    public static bool Prefix(PowerModel power, ref Task __result)
    {
        var relic = power.Owner.Player?.GetRelic<JianChiGu>();
        if (relic is null || !relic.TryPreserve(power))
        {
            return true;
        }

        __result = Task.CompletedTask;
        return false;
    }
}
