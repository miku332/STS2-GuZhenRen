using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class WuJinXuanGuangQiPower : ModPowerTemplate
{
    private sealed class StrengthLossState
    {
        public decimal PendingRestore;
    }

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/WuJinXuanGuangQiPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/WuJinXuanGuangQiPower_p.png");

    protected override object InitInternalData() => new StrengthLossState();

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (Amount <= 0 || !Owner.IsAlive || Owner.GetPower<SlowPower>() is null)
        {
            return;
        }

        var strengthBefore = Owner.GetPowerAmount<StrengthPower>();
        await PowerCmd.Apply<StrengthPower>(
            choiceContext,
            Owner,
            -Amount,
            cardPlay.Card.Owner.Creature,
            cardPlay.Card);

        var actualLoss = strengthBefore - Owner.GetPowerAmount<StrengthPower>();
        if (actualLoss > 0)
        {
            GetInternalData<StrengthLossState>().PendingRestore += actualLoss;
            Flash();
        }
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        var state = GetInternalData<StrengthLossState>();
        if (!participants.Contains(Owner) || state.PendingRestore <= 0)
        {
            return;
        }

        var amountToRestore = state.PendingRestore;
        state.PendingRestore = 0;
        await PowerCmd.Apply<StrengthPower>(
            choiceContext,
            Owner,
            amountToRestore,
            Owner,
            null);
    }
}
