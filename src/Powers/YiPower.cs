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
public sealed class YiPower : ModPowerTemplate
{
    private sealed class ConversionState
    {
        public bool IsConverting;
    }

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/YiPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/YiPower_p.png");

    protected override object InitInternalData() => new ConversionState();

    public override async Task AfterApplied(
        Creature? applier,
        CardModel? cardSource)
    {
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
        if (power == this && amount > 0 && Owner.Player is not null)
        {
            await ResolveThresholds(
                choiceContext,
                applier ?? Owner,
                cardSource);
        }
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

                await PlayerCmd.GainEnergy(1, Owner.Player);
                await PowerCmd.Apply<QingPower>(
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
