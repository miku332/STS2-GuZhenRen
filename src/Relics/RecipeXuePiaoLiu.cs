using GuZhenRen.CardPools;
using GuZhenRen.Cards;
using GuZhenRen.Systems;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class RecipeXuePiaoLiu : AbstractRecipeRelic
{
    private static readonly IReadOnlyList<RecipeIngredient> RequiredIngredients =
    [
        new(
            "ASSEMBLE_XUE_PIAO_LIU_XUE_ZOU",
            static card => card is XueZouGu { IsUpgraded: true }),
        new(
            "ASSEMBLE_XUE_PIAO_LIU_XIAN_GU",
            static card =>
                card is GuZhenRenCardTemplate { Rank: >= 6 and <= 9 }
                && GuZhenRenTagRules.HasEffectiveTag(card, GuZhenRenTags.XueDao))
    ];

    internal override CardModel RewardCard => ModelDb.Card<XuePiaoLiu>();

    internal override IReadOnlyList<RecipeIngredient> Ingredients =>
        RequiredIngredients;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/relics/Recipe_XueDao.png",
        IconOutlinePath:
            "res://GuZhenRen/images/relics/outline/Recipe_XueDao.png",
        BigIconPath: "res://GuZhenRen/images/relics/Recipe_XueDao.png");
}
