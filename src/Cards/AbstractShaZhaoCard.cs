using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace GuZhenRen.Cards;

public abstract class AbstractShaZhaoCard : GuZhenRenCardTemplate
{
    public override bool CanBeGeneratedInCombat => false;

    public override bool CanBeGeneratedByModifiers => false;

    protected AbstractShaZhaoCard(
        int energyCost,
        CardType cardType,
        CardRarity rarity,
        TargetType targetType,
        bool upgrades)
        : base(energyCost, cardType, rarity, targetType, upgrades)
    {
    }
}
