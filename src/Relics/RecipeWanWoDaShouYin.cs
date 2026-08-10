using GuZhenRen.CardPools;
using GuZhenRen.Cards;
using GuZhenRen.Systems;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class RecipeWanWoDaShouYin : AbstractRecipeRelic
{
    private static readonly IReadOnlyList<RecipeIngredient> RequiredIngredients =
    [
        new(
            "ASSEMBLE_WAN_WO_DA_SHOU_YIN_BA_SHAN",
            static card => card is BaShan { IsUpgraded: true }),
        new(
            "ASSEMBLE_WAN_WO_DA_SHOU_YIN_XIAN_GU",
            static card =>
                card is GuZhenRenCardTemplate { Rank: >= 6 }
                && GuZhenRenTagRules.HasEffectiveTag(card, GuZhenRenTags.LiDao))
    ];

    internal override CardModel RewardCard =>
        ModelDb.Card<WanWoDaShouYin>();

    internal override IReadOnlyList<RecipeIngredient> Ingredients =>
        RequiredIngredients;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/relics/Recipe_LiDao.png",
        IconOutlinePath:
            "res://GuZhenRen/images/relics/outline/Recipe_LiDao.png",
        BigIconPath: "res://GuZhenRen/images/relics/Recipe_LiDao.png");
}
