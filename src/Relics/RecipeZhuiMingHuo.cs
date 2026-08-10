using GuZhenRen.CardPools;
using GuZhenRen.Cards;
using GuZhenRen.Systems;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class RecipeZhuiMingHuo : AbstractRecipeRelic
{
    private static readonly IReadOnlyList<RecipeIngredient> RequiredIngredients =
    [
        new(
            "ASSEMBLE_ZHUI_MING_HUO_LIAO_YUAN",
            static card => card is LiaoYuanHuo { IsUpgraded: true }),
        new(
            "ASSEMBLE_ZHUI_MING_HUO_XIAN_GU",
            static card =>
                card is GuZhenRenCardTemplate { Rank: >= 6 }
                && card.Tags.Contains(GuZhenRenTags.YanDao))
    ];

    internal override CardModel RewardCard => ModelDb.Card<ZhuiMingHuo>();

    internal override IReadOnlyList<RecipeIngredient> Ingredients =>
        RequiredIngredients;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/relics/RecipeYanDao.png",
        IconOutlinePath:
            "res://GuZhenRen/images/relics/outline/RecipeYanDao.png",
        BigIconPath: "res://GuZhenRen/images/relics/RecipeYanDao.png");
}
