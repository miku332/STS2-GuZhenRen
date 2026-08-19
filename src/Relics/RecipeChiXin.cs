using GuZhenRen.CardPools;
using GuZhenRen.Cards;
using GuZhenRen.Systems;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class RecipeChiXin : AbstractRecipeRelic
{
    private static readonly IReadOnlyList<RecipeIngredient> RequiredIngredients =
    [
        new(
            "ASSEMBLE_CHI_XIN_CHI_LI",
            static card => card is ChiLi { IsUpgraded: true }),
        new(
            "ASSEMBLE_CHI_XIN_LU_DAO",
            static card =>
                card is GuZhenRenCardTemplate { Rank: >= 1 and <= 9 }
                && GuZhenRenTagRules.HasEffectiveTag(card, GuZhenRenTags.LuDao))
    ];

    internal override CardModel RewardCard => ModelDb.Card<ChiXin>();

    internal override IReadOnlyList<RecipeIngredient> Ingredients =>
        RequiredIngredients;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/relics/Recipe_ShiDao.png",
        IconOutlinePath:
            "res://GuZhenRen/images/relics/outline/Recipe_ShiDao.png",
        BigIconPath: "res://GuZhenRen/images/relics/Recipe_ShiDao.png");
}
