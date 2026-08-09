using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Patching.Models;

namespace GuZhenRen.Patches;

public sealed class NiLiuHePowerLookupPatch : IPatchMethod
{
    public static string PatchId => "ni_liu_he_power_lookup_reflection";

    public static string Description =>
        "Ni Liu He stacks reflected debuffs on the attacker";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(
            typeof(PowerCmd),
            nameof(PowerCmd.FindExistingInstanceForStacking),
            [typeof(PowerModel), typeof(Creature), typeof(Creature)])
    ];

    public static void Prefix(
        PowerModel basePower,
        ref Creature target,
        Creature? applier)
    {
        if (NiLiuHeReflectionState.TryRedirectDebuffLookup(
                basePower,
                target,
                applier,
                out var redirectedTarget))
        {
            target = redirectedTarget;
        }
    }
}
