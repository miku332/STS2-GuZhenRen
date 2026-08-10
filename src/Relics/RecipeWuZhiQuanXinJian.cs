using GuZhenRen.CardPools;
using GuZhenRen.Cards;
using GuZhenRen.Systems;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class RecipeWuZhiQuanXinJian : AbstractRecipeRelic
{
    private static readonly IReadOnlyList<RecipeIngredient> RequiredIngredients =
    [
        new(
            "ASSEMBLE_WU_ZHI_QUAN_XIN_JIAN_HUI_JIAN",
            static card => card is HuiJian { IsUpgraded: true }),
        new(
            "ASSEMBLE_WU_ZHI_QUAN_XIN_JIAN_XIAN_GU",
            static card =>
                card is GuZhenRenCardTemplate { Rank: >= 1 and <= 9 }
                && GuZhenRenTagRules.HasEffectiveTag(card, GuZhenRenTags.JianDao))
    ];

    internal override CardModel RewardCard =>
        ModelDb.Card<WuZhiQuanXinJian>();

    internal override IReadOnlyList<RecipeIngredient> Ingredients =>
        RequiredIngredients;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/relics/Recipe_JianDao.png",
        IconOutlinePath:
            "res://GuZhenRen/images/relics/outline/Recipe_JianDao.png",
        BigIconPath: "res://GuZhenRen/images/relics/Recipe_JianDao.png");
}
