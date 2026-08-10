using GuZhenRen.CardPools;
using GuZhenRen.Cards;
using GuZhenRen.Systems;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class RecipeXueJianLeng : AbstractRecipeRelic
{
    private static readonly IReadOnlyList<RecipeIngredient> RequiredIngredients =
    [
        new(
            "ASSEMBLE_XUE_JIAN_LENG_LENG_XUE",
            static card => card is LengXue { IsUpgraded: true }),
        new(
            "ASSEMBLE_XUE_JIAN_LENG_XIAN_GU",
            static card =>
                card is GuZhenRenCardTemplate { Rank: >= 1 and <= 9 }
                && GuZhenRenTagRules.HasEffectiveTag(card, GuZhenRenTags.XueDao))
    ];

    internal override CardModel RewardCard => ModelDb.Card<XueJianLeng>();

    internal override IReadOnlyList<RecipeIngredient> Ingredients =>
        RequiredIngredients;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/relics/Recipe_XueDao.png",
        IconOutlinePath:
            "res://GuZhenRen/images/relics/outline/Recipe_XueDao.png",
        BigIconPath: "res://GuZhenRen/images/relics/Recipe_XueDao.png");
}
