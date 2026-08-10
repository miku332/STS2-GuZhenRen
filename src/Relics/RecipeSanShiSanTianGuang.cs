using GuZhenRen.CardPools;
using GuZhenRen.Cards;
using GuZhenRen.Systems;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class RecipeSanShiSanTianGuang : AbstractRecipeRelic
{
    private static readonly IReadOnlyList<RecipeIngredient> RequiredIngredients =
    [
        new(
            "ASSEMBLE_SAN_SHI_SAN_TIAN_GUANG_JIANG_HE",
            static card => card is JiangHeRiXiaGu { IsUpgraded: true }),
        new(
            "ASSEMBLE_SAN_SHI_SAN_TIAN_GUANG_XIAN_GU",
            static card =>
                card is GuZhenRenCardTemplate { Rank: >= 1 and <= 9 }
                && GuZhenRenTagRules.HasEffectiveTag(card, GuZhenRenTags.GuangDao))
    ];

    internal override CardModel RewardCard =>
        ModelDb.Card<SanShiSanTianGuang>();

    internal override IReadOnlyList<RecipeIngredient> Ingredients =>
        RequiredIngredients;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/relics/Recipe_GuangDao.png",
        IconOutlinePath:
            "res://GuZhenRen/images/relics/outline/Recipe_GuangDao.png",
        BigIconPath: "res://GuZhenRen/images/relics/Recipe_GuangDao.png");
}
