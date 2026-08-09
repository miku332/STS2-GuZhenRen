using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Patching.Models;

namespace GuZhenRen.Patches;

public sealed class NiLiuHePowerModifyPatch : IPatchMethod
{
    public static string PatchId => "ni_liu_he_power_modify_reflection";

    public static string Description =>
        "Ni Liu He redirects negative stacks of existing powers";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(
            typeof(PowerCmd),
            nameof(PowerCmd.ModifyAmount),
            [
                typeof(PlayerChoiceContext),
                typeof(PowerModel),
                typeof(decimal),
                typeof(Creature),
                typeof(CardModel),
                typeof(bool)
            ])
    ];

    public static bool Prefix(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal offset,
        Creature? applier,
        CardModel? cardSource,
        bool silent,
        ref Task<int> __result)
    {
        if (!NiLiuHeReflectionState.WasLastAttackReflected(
                power.Owner,
                applier)
            || power.GetTypeForAmount(offset)
                != MegaCrit.Sts2.Core.Entities.Powers.PowerType.Debuff)
        {
            return true;
        }

        __result = ApplyToAttacker(
            choiceContext,
            power,
            offset,
            applier!,
            cardSource,
            silent);
        return false;
    }

    private static async Task<int> ApplyToAttacker(
        PlayerChoiceContext choiceContext,
        PowerModel originalPower,
        decimal amount,
        Creature attacker,
        CardModel? cardSource,
        bool silent)
    {
        var canonical = ModelDb.GetById<PowerModel>(originalPower.Id);
        await PowerCmd.Apply(
            choiceContext,
            canonical.ToMutable(),
            attacker,
            amount,
            attacker,
            cardSource,
            silent);

        return attacker.GetPower(originalPower.Id)?.Amount ?? 0;
    }
}
