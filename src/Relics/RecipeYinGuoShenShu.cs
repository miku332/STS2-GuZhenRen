using GuZhenRen.CardPools;
using GuZhenRen.Cards;
using GuZhenRen.Systems;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class RecipeYinGuoShenShu : AbstractRecipeRelic
{
    private static readonly IReadOnlyList<RecipeIngredient> RequiredIngredients =
    [
        new(
            "ASSEMBLE_YIN_GUO_SHEN_SHU_TIAN_YUAN_BAO_LIAN",
            static card => card is TianYuanBaoLian { IsUpgraded: true }),
        new(
            "ASSEMBLE_YIN_GUO_SHEN_SHU_XIAN_GU",
            static card =>
                card is GuZhenRenCardTemplate { Rank: >= 6 }
                && (GuZhenRenTagRules.HasEffectiveTag(card, GuZhenRenTags.LuDao)
                    || GuZhenRenTagRules.HasEffectiveTag(card, GuZhenRenTags.YunDao))),
    ];

    internal override CardModel RewardCard => ModelDb.Card<YinGuoShenShu>();

    internal override IReadOnlyList<RecipeIngredient> Ingredients =>
        RequiredIngredients;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/relics/Recipe_LuDao.png",
        IconOutlinePath: "res://GuZhenRen/images/relics/outline/Recipe_LuDao.png",
        BigIconPath: "res://GuZhenRen/images/relics/Recipe_LuDao.png");
}
