using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class FeiXingPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/FeiXingPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/FeiXingPower_p.png");

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (target != Owner || Amount <= 0 || !props.IsPoweredAttack())
        {
            return 1m;
        }

        return 0.5m;
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
            || !props.IsPoweredAttack())
        {
            return;
        }

        Flash();
        await PowerCmd.ModifyAmount(choiceContext, this, -1, Owner, cardSource);
        await RemoveLinkedPowerIfFlightEnded(choiceContext, cardSource);
    }

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        if (power == this)
        {
            await RemoveLinkedPowerIfFlightEnded(choiceContext, cardSource);
        }
    }

    private async Task RemoveLinkedPowerIfFlightEnded(
        PlayerChoiceContext choiceContext,
        CardModel? cardSource)
    {
        if (Amount > 0)
        {
            return;
        }

        var zhenChi = Owner.GetPower<ZhenChiGaoFeiPower>();
        if (zhenChi is not null)
        {
            await PowerCmd.Remove(zhenChi);
        }
    }
}
