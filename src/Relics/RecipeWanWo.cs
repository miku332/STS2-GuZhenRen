using GuZhenRen.CardPools;
using GuZhenRen.Cards;
using GuZhenRen.Systems;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class RecipeWanWo : AbstractRecipeRelic
{
    private static readonly IReadOnlyList<RecipeIngredient> RequiredIngredients =
    [
        new(
            "ASSEMBLE_WAN_WO_WO_LI",
            static card => card is WoLi { IsUpgraded: true }),
        new(
            "ASSEMBLE_WAN_WO_XIAN_GU",
            static card =>
                card is GuZhenRenCardTemplate { Rank: >= 1 and <= 9 }
                && GuZhenRenTagRules.HasEffectiveTag(card, GuZhenRenTags.LiDao))
    ];

    internal override CardModel RewardCard => ModelDb.Card<WanWo>();

    internal override IReadOnlyList<RecipeIngredient> Ingredients =>
        RequiredIngredients;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/relics/Recipe_LiDao.png",
        IconOutlinePath:
            "res://GuZhenRen/images/relics/outline/Recipe_LiDao.png",
        BigIconPath: "res://GuZhenRen/images/relics/Recipe_LiDao.png");
}
