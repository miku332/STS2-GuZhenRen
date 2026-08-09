using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class NianPower : ModPowerTemplate
{
    private sealed class ConversionState
    {
        public bool IsConverting;
    }

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/NianPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/NianPower_p.png");

    protected override object InitInternalData() => new ConversionState();

    public override async Task AfterApplied(
        Creature? applier,
        CardModel? cardSource)
    {
        if (await TryConvertByZhiZhang(
                new ThrowingPlayerChoiceContext(),
                Amount,
                applier,
                cardSource))
        {
            return;
        }

        if (await TryBlockByNianTouShouZu())
        {
            return;
        }

        if (Amount > 0 && Owner.Player is not null)
        {
            await ResolveThresholds(
                new ThrowingPlayerChoiceContext(),
                applier ?? Owner,
                cardSource);
        }
    }

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        if (power == this
            && amount > 0
            && Owner.GetPower<NianPower>() == this
            && await TryConvertByZhiZhang(
                choiceContext,
                amount,
                applier,
                cardSource))
        {
            return;
        }

        if (power == this && amount > 0 && await TryBlockByNianTouShouZu())
        {
            return;
        }

        if (power == this
            && amount > 0
            && Owner.Player is not null
            && Owner.GetPower<NianPower>() == this)
        {
            await XingLuoQiBuPower.TriggerBeforeNianGain(Owner, amount);
            await ResolveThresholds(
                choiceContext,
                applier ?? Owner,
                cardSource);
        }
    }

    private async Task<bool> TryConvertByZhiZhang(
        PlayerChoiceContext choiceContext,
        decimal gainedAmount,
        Creature? applier,
        CardModel? cardSource)
    {
        var converter = Owner.GetPower<ZhiZhangPower>();
        if (converter is null || gainedAmount <= 0)
        {
            return false;
        }

        converter.FlashConversion();
        await PowerCmd.Apply<TemporaryHpPower>(
            choiceContext,
            Owner,
            gainedAmount,
            applier ?? Owner,
            cardSource);

        if (Owner.GetPower<NianPower>() == this)
        {
            SetAmount(0, false);
            await PowerCmd.Remove(this);
        }

        return true;
    }

    private async Task<bool> TryBlockByNianTouShouZu()
    {
        var blocker = Owner.GetPower<NianTouShouZuPower>();
        if (blocker is null || Amount <= 0)
        {
            return false;
        }

        blocker.FlashBlocked();
        SetAmount(0, false);
        await PowerCmd.Remove(this);
        return true;
    }

    private async Task ResolveThresholds(
        PlayerChoiceContext choiceContext,
        Creature applier,
        CardModel? cardSource)
    {
        var state = GetInternalData<ConversionState>();
        if (state.IsConverting || Owner.Player is null)
        {
            return;
        }

        state.IsConverting = true;
        try
        {
            while (Amount >= 3)
            {
                Flash();
                SetAmount((int)Amount - 3, false);

                await CardPileCmd.Draw(choiceContext, 1, Owner.Player);
                await PowerCmd.Apply<YiPower>(
                    choiceContext,
                    Owner,
                    1,
                    applier,
                    cardSource);
                await ZhuanYiPower.TriggerConversion(Owner, applier, cardSource);
            }
        }
        finally
        {
            state.IsConverting = false;
        }
    }
}
