using GuZhenRen.CardPools;
using GuZhenRen.Cards;
using GuZhenRen.Systems;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class RecipeGuangYinFeiRen : AbstractRecipeRelic
{
    private static readonly IReadOnlyList<RecipeIngredient> RequiredIngredients =
    [
        new(
            "ASSEMBLE_GUANG_YIN_FEI_REN_REN_GU",
            static card => card is RenGu),
        new(
            "ASSEMBLE_GUANG_YIN_FEI_REN_XIAN_GU",
            static card =>
                card is GuZhenRenCardTemplate { Rank: >= 6 and <= 9 }
                && GuZhenRenTagRules.HasEffectiveTag(
                    card,
                    GuZhenRenTags.ZhouDao))
    ];

    internal override CardModel RewardCard => ModelDb.Card<GuangYinFeiRen>();

    internal override IReadOnlyList<RecipeIngredient> Ingredients =>
        RequiredIngredients;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/relics/Recipe_ZhouDao.png",
        IconOutlinePath:
            "res://GuZhenRen/images/relics/outline/Recipe_ZhouDao.png",
        BigIconPath: "res://GuZhenRen/images/relics/Recipe_ZhouDao.png");
}
