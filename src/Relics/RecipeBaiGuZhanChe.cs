using GuZhenRen.CardPools;
using GuZhenRen.Cards;
using GuZhenRen.Systems;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class RecipeBaiGuZhanChe : AbstractRecipeRelic
{
    private static readonly IReadOnlyList<RecipeIngredient> RequiredIngredients =
    [
        new(
            "ASSEMBLE_BAI_GU_ZHAN_CHE_ZHAN_GU_CHE_LUN",
            static card => card is ZhanGuCheLun { IsUpgraded: true }),
        new(
            "ASSEMBLE_BAI_GU_ZHAN_CHE_XIAN_GU",
            static card =>
                card is GuZhenRenCardTemplate { Rank: >= 1 and <= 9 }
                && GuZhenRenTagRules.HasEffectiveTag(
                    card,
                    GuZhenRenTags.GuDao))
    ];

    internal override CardModel RewardCard => ModelDb.Card<BaiGuZhanChe>();

    internal override IReadOnlyList<RecipeIngredient> Ingredients =>
        RequiredIngredients;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/relics/Recipe_GuDao.png",
        IconOutlinePath:
            "res://GuZhenRen/images/relics/outline/Recipe_GuDao.png",
        BigIconPath: "res://GuZhenRen/images/relics/Recipe_GuDao.png");
}
