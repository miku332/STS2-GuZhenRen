using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Patching.Models;

namespace GuZhenRen.Patches;

public sealed class QiHuPowerApplyPatch : IPatchMethod
{
    public static string PatchId => "qi_hu_power_apply_redirection";

    public static string Description =>
        "Qi Hu redirects debuffs from Long Gong to the Qi Wall";

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
        decimal amount)
    {
        if (QiHuState.TryRedirectPower(
                power,
                target,
                amount,
                out var redirectedTarget))
        {
            target = redirectedTarget;
        }
    }
}
