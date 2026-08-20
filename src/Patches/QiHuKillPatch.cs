using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using STS2RitsuLib.Patching.Models;

namespace GuZhenRen.Patches;

public sealed class QiHuKillPatch : IPatchMethod
{
    public static string PatchId => "qi_hu_kill_redirection";

    public static string Description =>
        "Qi Hu redirects direct kill effects from Long Gong to the Qi Wall";

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
        if (force
            || !QiHuState.TryRedirectDamage(creature, out var protector))
        {
            return true;
        }

        __result = CreatureCmd.Kill(protector);
        return false;
    }
}
