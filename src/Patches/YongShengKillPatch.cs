using GuZhenRen.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using STS2RitsuLib.Patching.Models;

namespace GuZhenRen.Patches;

public sealed class YongShengKillPatch : IPatchMethod
{
    public static string PatchId => "yong-sheng-prevent-direct-death";

    public static string Description =>
        "Yong Sheng prevents non-forced direct death effects.";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(
            typeof(CreatureCmd),
            nameof(CreatureCmd.Kill),
            [typeof(Creature), typeof(bool)])
    ];

    public static bool Prefix(
        Creature creature,
        bool force,
        ref Task __result)
    {
        var relic = creature.Player?.GetRelic<XianQiao10>();
        if (force || relic is null)
        {
            return true;
        }

        relic.Flash();
        __result = Task.CompletedTask;
        return false;
    }
}
