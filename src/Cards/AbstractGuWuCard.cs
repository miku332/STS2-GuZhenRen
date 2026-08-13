using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
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

    public static async Task ReturnAllToHand(Player player)
    {
        foreach (var pileType in new[]
                 {
                     PileType.Draw,
                     PileType.Discard,
                     PileType.Exhaust
                 })
        {
            var cards = pileType
                .GetPile(player)
                .Cards
                .OfType<AbstractGuWuCard>()
                .ToList();
            foreach (var card in cards)
            {
                await CardPileCmd.Add(
                    card,
                    PileType.Hand,
                    CardPilePosition.Bottom);
            }
        }
    }

    protected override void OnUpgrade()
    {
    }
}
