using GuZhenRen.CardPools;
using GuZhenRen.Cards;
using GuZhenRen.Systems;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class RecipeXingXiuQiPan : AbstractRecipeRelic
{
    private static readonly IReadOnlyList<RecipeIngredient> RequiredIngredients =
    [
        new(
            "ASSEMBLE_XING_XIU_ZHI_HUI",
            static card => card is ZhiHuiGu),
        new(
            "ASSEMBLE_XING_XIU_XIAN_GU",
            static card =>
                card is GuZhenRenCardTemplate { Rank: >= 6 }
                && card.Tags.Contains(GuZhenRenTags.ZhiDao))
    ];

    internal override CardModel RewardCard => ModelDb.Card<XingXiuQiPan>();

    internal override IReadOnlyList<RecipeIngredient> Ingredients =>
        RequiredIngredients;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/relics/RecipeZhiDao.png",
        IconOutlinePath:
            "res://GuZhenRen/images/relics/outline/RecipeZhiDao.png",
        BigIconPath: "res://GuZhenRen/images/relics/RecipeZhiDao.png");
}
