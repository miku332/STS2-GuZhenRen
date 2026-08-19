using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class YunTouZhuanXiangPower : ModPowerTemplate
{
    private const decimal DecayPerTurn = 20m;

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/YunTouZhuanXiangPower.png",
        BigIconPath:
            "res://GuZhenRen/images/powers/YunTouZhuanXiangPower_p.png");

    public override Task AfterApplied(
        Creature? applier,
        CardModel? cardSource)
    {
        if (Amount > 100)
        {
            SetAmount(100);
        }

        return Task.CompletedTask;
    }

    public override Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        if (power == this && Amount > 100)
        {
            SetAmount(100);
        }

        return Task.CompletedTask;
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Enemy
            || Amount <= 0
            || !Owner.IsAlive
            || !participants.Contains(Owner))
        {
            return;
        }

        Flash();
        var player = Applier?.Player
            ?? Owner.CombatState?.Players.FirstOrDefault();
        if (player is not null
            && player.RunState.Rng.CombatTargets.NextFloat(100f) < (float)Amount)
        {
            await CreatureCmd.Stun(Owner);
        }

        if (Amount <= DecayPerTurn)
        {
            await PowerCmd.Remove(this);
            return;
        }

        await PowerCmd.ModifyAmount(
            choiceContext,
            this,
            -DecayPerTurn,
            null,
            null);
    }
}
