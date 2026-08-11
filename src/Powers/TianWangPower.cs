using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class TianWangPower : ModPowerTemplate
{
    private const int CardsToRelease = 3;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/TianWangPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/TianWangPower_p.png");

    public override bool ShouldPlay(CardModel card, AutoPlayType autoPlayType)
    {
        return !Owner.IsAlive
            || Amount <= 0
            || card.Owner.Creature != GetAffectedPlayer()
            || card.Type != CardType.Attack;
    }

    public override Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (!Owner.IsAlive
            || Amount <= 0
            || cardPlay.Card.Owner.Creature != GetAffectedPlayer())
        {
            return Task.CompletedTask;
        }

        Flash();
        SetAmount(Math.Max(0, Amount - 1));
        return Task.CompletedTask;
    }

    public override Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side == CombatSide.Enemy
            && Owner.IsAlive
            && participants.Contains(Owner))
        {
            Flash();
            SetAmount(CardsToRelease);
        }

        return Task.CompletedTask;
    }

    private Creature? GetAffectedPlayer() =>
        Applier?.Player is not null
            ? Applier
            : Owner.CombatState?.Players.FirstOrDefault()?.Creature;
}
