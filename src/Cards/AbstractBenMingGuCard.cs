using MegaCrit.Sts2.Core.Entities.Cards;

namespace GuZhenRen.Cards;

public abstract class AbstractBenMingGuCard : GuZhenRenCardTemplate
{
    protected virtual int MaxRank => 9;

    public sealed override int Rank => CurrentUpgradeLevel + 1;

    public sealed override int MaxUpgradeLevel => Math.Max(1, MaxRank - 1);

    public override bool CanBeGeneratedInCombat => false;

    public override bool CanBeGeneratedByModifiers => false;

    protected AbstractBenMingGuCard(
        int energyCost,
        CardType cardType,
        CardRarity rarity,
        TargetType targetType)
        : base(energyCost, cardType, rarity, targetType, true)
    {
    }
}
