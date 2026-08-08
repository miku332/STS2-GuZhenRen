using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.Tags;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class ShanYaoPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/ShanYaoPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/ShanYaoPower_p.png");

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (dealer != Owner
            || cardSource is null
            || !cardSource.Tags.Contains(GuZhenRenTags.GuangDao)
            || !props.IsPoweredAttack())
        {
            return 1m;
        }

        return 1m + Amount * 0.5m;
    }

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != Owner
            || cardPlay.Card.Type != CardType.Attack
            || !cardPlay.Card.Tags.Contains(GuZhenRenTags.GuangDao))
        {
            return;
        }

        var riGuang = Owner.GetPower<RiGuangPower>();
        if (riGuang is not null && riGuang.Amount > 0)
        {
            await PowerCmd.Decrement(riGuang);
            return;
        }

        await PowerCmd.Remove(this);
    }
}
