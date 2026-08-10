using GuZhenRen.CardPools;
using GuZhenRen.Cards;
using GuZhenRen.Systems;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class RecipeAnTuZhongShanBao : AbstractRecipeRelic
{
    private static readonly RecipeIngredient TuDaoIngredient = new(
        "ASSEMBLE_AN_TU_ZHONG_SHAN_BAO_TU_DAO",
        static card =>
            card is GuZhenRenCardTemplate { Rank: >= 1 and <= 9 }
            && GuZhenRenTagRules.HasEffectiveTag(
                card,
                GuZhenRenTags.TuDao));

    private static readonly IReadOnlyList<RecipeIngredient> RequiredIngredients =
    [
        TuDaoIngredient,
        TuDaoIngredient,
        TuDaoIngredient
    ];

    internal override CardModel RewardCard =>
        ModelDb.Card<AnTuZhongShanBao>();

    internal override IReadOnlyList<RecipeIngredient> Ingredients =>
        RequiredIngredients;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/relics/Recipe_TuDao.png",
        IconOutlinePath:
            "res://GuZhenRen/images/relics/outline/Recipe_TuDao.png",
        BigIconPath: "res://GuZhenRen/images/relics/Recipe_TuDao.png");
}
