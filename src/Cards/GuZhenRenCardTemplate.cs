using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Cards;

public abstract class GuZhenRenCardTemplate : ModCardTemplate
{
    private readonly bool _upgrades;

    public virtual int Rank => 1;

    public override int MaxUpgradeLevel => _upgrades ? base.MaxUpgradeLevel : 0;

    protected GuZhenRenCardTemplate(
        int energyCost,
        CardType cardType,
        CardRarity rarity,
        TargetType targetType,
        bool upgrades)
        : base(energyCost, cardType, rarity, targetType)
    {
        _upgrades = upgrades;
    }
}
