using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Cards;

public abstract class GuZhenRenCardTemplate : ModCardTemplate
{
    private readonly bool _upgrades;

    private static readonly IHoverTip RankHoverTip = new HoverTip(
        new LocString(
            "card_keywords",
            "GU_ZHEN_REN_KEYWORD_PIN_JIE.title"),
        new LocString(
            "card_keywords",
            "GU_ZHEN_REN_KEYWORD_PIN_JIE.description"));

    protected readonly record struct GeneratedCardPreview(
        CardModel Card,
        bool Upgraded);

    public virtual int Rank => 1;

    public override int MaxUpgradeLevel => _upgrades ? base.MaxUpgradeLevel : 0;

    protected virtual IEnumerable<GeneratedCardPreview> GeneratedCardPreviews =>
        [];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            if (Rank > 0)
            {
                yield return RankHoverTip;
            }

            foreach (var preview in GeneratedCardPreviews)
            {
                yield return HoverTipFactory.FromCard(
                    preview.Card,
                    preview.Upgraded);
            }
        }
    }

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

    protected static GeneratedCardPreview PreviewCard<T>(
        bool upgraded = false)
        where T : CardModel =>
        new(ModelDb.Card<T>(), upgraded);
}
