using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class LangJingPower : ModPowerTemplate
{
    private int _reduction;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override int DisplayAmount => _reduction;

    public override LocString Description
    {
        get
        {
            var description = base.Description;
            description.Add("Reduction", _reduction);
            return description;
        }
    }

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/LangJingPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/LangJingPower_p.png");

    public override decimal ModifyDamageAdditive(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (target != Owner
            || _reduction <= 0
            || amount <= 0
            || props.HasFlag(ValueProp.Unblockable))
        {
            return 0m;
        }

        return -Math.Min(amount, _reduction);
    }

    public override Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target == Owner
            && result.UnblockedDamage > 0
            && !props.HasFlag(ValueProp.Unblockable))
        {
            Flash();
            _reduction++;
            InvokeDisplayAmountChanged();
        }

        return Task.CompletedTask;
    }

    public override Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side == CombatSide.Enemy
            && Owner.IsAlive
            && participants.Contains(Owner)
            && _reduction > 0)
        {
            Flash();
            _reduction = 0;
            InvokeDisplayAmountChanged();
        }

        return Task.CompletedTask;
    }
}
