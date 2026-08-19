using GuZhenRen.CardPools;
using GuZhenRen.Cards;
using GuZhenRen.Systems;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class RecipeNiePanHuo : AbstractRecipeRelic
{
    private static readonly IReadOnlyList<RecipeIngredient> RequiredIngredients =
    [
        new(
            "ASSEMBLE_NIE_PAN_HUO_YU_HUO",
            static card => card is YuHuo { IsUpgraded: true }),
        new(
            "ASSEMBLE_NIE_PAN_HUO_XIAN_GU",
            static card =>
                card is GuZhenRenCardTemplate { Rank: >= 6 }
                && (GuZhenRenTagRules.HasEffectiveTag(card, GuZhenRenTags.YanDao)
                    || GuZhenRenTagRules.HasEffectiveTag(
                        card,
                        GuZhenRenTags.ZhouDao)))
    ];

    internal override CardModel RewardCard => ModelDb.Card<NiePanHuo>();

    internal override IReadOnlyList<RecipeIngredient> Ingredients =>
        RequiredIngredients;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/relics/Recipe_YanDao.png",
        IconOutlinePath:
            "res://GuZhenRen/images/relics/outline/Recipe_YanDao.png",
        BigIconPath: "res://GuZhenRen/images/relics/Recipe_YanDao.png");
}
