using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Patching.Models;

namespace GuZhenRen.Patches;

public sealed class NiLiuHePowerApplyPatch : IPatchMethod
{
    public static string PatchId => "ni_liu_he_power_apply_reflection";

    public static string Description =>
        "Ni Liu He redirects debuffs attached to a reflected attack";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(
            typeof(PowerCmd),
            nameof(PowerCmd.Apply),
            [
                typeof(PlayerChoiceContext),
                typeof(PowerModel),
                typeof(Creature),
                typeof(decimal),
                typeof(Creature),
                typeof(CardModel),
                typeof(bool)
            ])
    ];

    public static void Prefix(
        PowerModel power,
        ref Creature target,
        decimal amount,
        Creature? applier)
    {
        if (NiLiuHeReflectionState.TryRedirectPower(
                power,
                target,
                amount,
                applier,
                out var redirectedTarget))
        {
            target = redirectedTarget;
        }
    }
}
