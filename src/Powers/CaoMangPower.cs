using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class CaoMangPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/CaoMangPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/CaoMangPower_p.png");

    public override async Task AfterApplied(
        Creature? applier,
        CardModel? cardSource)
    {
        if (Amount <= 0)
        {
            return;
        }

        await PowerCmd.Apply<StrengthPower>(
            new ThrowingPlayerChoiceContext(),
            Owner,
            Amount,
            Owner,
            null);
    }

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner
            || Amount <= 0
            || result.UnblockedDamage <= 0
            || !FenShaoPower.IsResolvingBurningDamage)
        {
            return;
        }

        Flash();
        var strength = Owner.GetPower<StrengthPower>();
        if (strength is not null && strength.Amount > 0)
        {
            await PowerCmd.ModifyAmount(
                choiceContext,
                strength,
                -1,
                Owner,
                null);
        }

        if (Amount <= 1)
        {
            await PowerCmd.Remove(this);
        }
        else
        {
            await PowerCmd.Decrement(this);
        }
    }
}
