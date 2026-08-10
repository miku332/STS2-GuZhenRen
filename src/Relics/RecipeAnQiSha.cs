using GuZhenRen.CardPools;
using GuZhenRen.Cards;
using GuZhenRen.Systems;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class RecipeAnQiSha : AbstractRecipeRelic
{
    private static readonly IReadOnlyList<RecipeIngredient> RequiredIngredients =
    [
        new(
            "ASSEMBLE_AN_QI_SHA_FEI_JIAN",
            static card => card is FeiJian { IsUpgraded: true }),
        new(
            "ASSEMBLE_AN_QI_SHA_JIAN_YING",
            static card => card is JianYingGu or DuoChongJianYingGu or DieYingGu)
    ];

    internal override CardModel RewardCard => ModelDb.Card<AnQiSha>();

    internal override IReadOnlyList<RecipeIngredient> Ingredients =>
        RequiredIngredients;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/relics/Recipe_JianDao.png",
        IconOutlinePath:
            "res://GuZhenRen/images/relics/outline/Recipe_JianDao.png",
        BigIconPath: "res://GuZhenRen/images/relics/Recipe_JianDao.png");
}
