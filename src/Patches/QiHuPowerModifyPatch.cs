using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Patching.Models;

namespace GuZhenRen.Patches;

public sealed class QiHuPowerModifyPatch : IPatchMethod
{
    public static string PatchId => "qi_hu_power_modify_redirection";

    public static string Description =>
        "Qi Hu redirects negative changes to Long Gong's existing powers";

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
        if (power.GetTypeForAmount(offset) != PowerType.Debuff
            || !QiHuState.TryRedirectPower(
                power,
                power.Owner,
                offset,
                out var protector))
        {
            return true;
        }

        __result = ApplyToProtector(
            choiceContext,
            power,
            offset,
            protector,
            applier,
            cardSource,
            silent);
        return false;
    }

    private static async Task<int> ApplyToProtector(
        PlayerChoiceContext choiceContext,
        PowerModel originalPower,
        decimal amount,
        Creature protector,
        Creature? applier,
        CardModel? cardSource,
        bool silent)
    {
        var canonical = ModelDb.GetById<PowerModel>(originalPower.Id);
        await PowerCmd.Apply(
            choiceContext,
            canonical.ToMutable(),
            protector,
            amount,
            applier,
            cardSource,
            silent);

        return protector.GetPower(originalPower.Id)?.Amount ?? 0;
    }
}
