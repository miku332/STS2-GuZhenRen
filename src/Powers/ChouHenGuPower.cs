using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands.Builders;
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
public sealed class ChouHenGuPower : ModPowerTemplate
{
    private bool _triggeredThisRound;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/ChouHenGuPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/ChouHenGuPower_p.png");

    public override Task AfterApplied(
        Creature? applier,
        CardModel? cardSource)
    {
        // PowerCmd ignores applications with amount 0. The application amount
        // only creates this persistent power; recorded damage starts at zero.
        SetAmount(0);
        return Task.CompletedTask;
    }

    public override Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner
            || !Owner.IsAlive
            || _triggeredThisRound
            || dealer?.Player is null
            || !props.IsPoweredAttack()
            || result.UnblockedDamage <= 0)
        {
            return Task.CompletedTask;
        }

        Flash();
        SetAmount(Amount + result.UnblockedDamage);
        _triggeredThisRound = true;
        return Task.CompletedTask;
    }

    public override decimal ModifyDamageAdditive(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        return dealer == Owner
            && Owner.IsAlive
            && Amount > 0
            && props.IsPoweredAttack()
                ? Amount
                : 0m;
    }

    public override Task AfterAttack(
        PlayerChoiceContext choiceContext,
        AttackCommand command)
    {
        if (command.Attacker == Owner
            && Amount > 0
            && command.DamageProps.IsPoweredAttack()
            && command.Results.SelectMany(results => results).Any())
        {
            Flash();
            SetAmount(0);
        }

        return Task.CompletedTask;
    }

    public override Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Enemy)
        {
            _triggeredThisRound = false;
        }

        return Task.CompletedTask;
    }
}
