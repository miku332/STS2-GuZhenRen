using GuZhenRen.CardPools;
using GuZhenRen.Cards;
using GuZhenRen.Systems;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class RecipeWeiLaiShen : AbstractRecipeRelic
{
    private static readonly IReadOnlyList<RecipeIngredient> RequiredIngredients =
    [
        new(
            "ASSEMBLE_WEI_LAI_SHEN_SHI_ZHEN",
            static card => card is ShiZhen { IsUpgraded: true }),
        new(
            "ASSEMBLE_WEI_LAI_SHEN_XIAN_GU",
            static card =>
                card is GuZhenRenCardTemplate { Rank: >= 6 }
                && GuZhenRenTagRules.HasEffectiveTag(
                    card,
                    GuZhenRenTags.ZhouDao))
    ];

    internal override CardModel RewardCard => ModelDb.Card<WeiLaiShen>();

    internal override IReadOnlyList<RecipeIngredient> Ingredients =>
        RequiredIngredients;

    internal override bool CanBeBorrowedByWeiLaiShen => false;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/relics/Recipe_ZhouDao.png",
        IconOutlinePath:
            "res://GuZhenRen/images/relics/outline/Recipe_ZhouDao.png",
        BigIconPath: "res://GuZhenRen/images/relics/Recipe_ZhouDao.png");
}
