using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
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
public sealed class FenShaoPower : ModPowerTemplate
{
    private static bool s_isSpreading;

    private sealed class BurnState
    {
        public bool SkipNextPositiveAmountChange;
    }

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/FenShaoPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/FenShaoPower_p.png");

    protected override object InitInternalData() => new BurnState();

    public override async Task AfterApplied(
        Creature? applier,
        CardModel? cardSource)
    {
        var state = GetInternalData<BurnState>();
        state.SkipNextPositiveAmountChange = true;

        await TriggerBurningDamage(
            new ThrowingPlayerChoiceContext(),
            applier,
            cardSource);
        await TriggerXingHuoLiaoYuan(
            new ThrowingPlayerChoiceContext(),
            Amount,
            applier,
            cardSource);
    }

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        if (power != this)
        {
            return;
        }

        var state = GetInternalData<BurnState>();
        if (amount > 0 && state.SkipNextPositiveAmountChange)
        {
            state.SkipNextPositiveAmountChange = false;
            return;
        }

        await TriggerBurningDamage(choiceContext, applier, cardSource);
        if (amount > 0)
        {
            await TriggerXingHuoLiaoYuan(choiceContext, amount, applier, cardSource);
        }
    }

    private async Task TriggerBurningDamage(
        PlayerChoiceContext choiceContext,
        Creature? applier,
        CardModel? cardSource)
    {
        if (Amount <= 0 || !Owner.IsAlive)
        {
            return;
        }

        Flash();
        Entry.Logger.Info($"[FenShao] Burning damage: amount={Amount}, source={cardSource?.Id.ToString() ?? "<none>"}");
        await CreatureCmd.Damage(
            choiceContext,
            Owner,
            Amount,
            ValueProp.Unpowered,
            applier,
            cardSource);
    }

    private async Task TriggerXingHuoLiaoYuan(
        PlayerChoiceContext choiceContext,
        decimal amountApplied,
        Creature? applier,
        CardModel? cardSource)
    {
        if (amountApplied <= 0 || s_isSpreading)
        {
            return;
        }

        var spreadPower = Owner.GetPower<XingHuoLiaoYuanPower>();
        if (spreadPower is null)
        {
            return;
        }

        s_isSpreading = true;
        try
        {
            await spreadPower.Spread(choiceContext, amountApplied, applier, cardSource);
        }
        finally
        {
            s_isSpreading = false;
        }
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (Amount <= 0)
        {
            return;
        }

        var remaining = (int)Math.Ceiling(Amount / 2m);
        var reduction = (int)Amount - remaining;
        if (reduction > 0 && participants.Contains(Owner))
        {
            await PowerCmd.ModifyAmount(choiceContext, this, -reduction, null, null);
        }
    }
}
