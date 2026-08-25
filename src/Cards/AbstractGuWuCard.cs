using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using GuZhenRen.Tags;

namespace GuZhenRen.Cards;

public abstract class AbstractGuWuCard : AbstractShaZhaoCard
{
    public override int Rank => 0;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    public override bool CanBeGeneratedInCombat => false;

    public override bool CanBeGeneratedByModifiers => false;

    protected AbstractGuWuCard(
        int energyCost,
        CardType cardType,
        TargetType targetType)
        : base(
            energyCost,
            cardType,
            CardRarity.Token,
            targetType,
            false)
    {
    }

    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side != CombatSide.Player
            || !participants.Contains(Owner.Creature)
            || Pile?.Type is not (PileType.Draw or PileType.Discard or PileType.Exhaust))
        {
            return;
        }

        await CardPileCmd.Add(
            this,
            PileType.Hand,
            CardPilePosition.Bottom);
    }

    protected override void OnUpgrade()
    {
    }
}
