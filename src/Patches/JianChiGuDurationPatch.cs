using GuZhenRen.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Patching.Models;

namespace GuZhenRen.Patches;

public sealed class JianChiGuDurationPatch : IPatchMethod
{
    public static string PatchId => "jian-chi-gu-duration";

    public static string Description =>
        "Jian Chi Gu preserves each naturally expiring buff once.";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(
            typeof(PowerCmd),
            nameof(PowerCmd.TickDownDuration),
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
