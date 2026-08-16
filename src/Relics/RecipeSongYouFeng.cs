using GuZhenRen.CardPools;
using GuZhenRen.Cards;
using GuZhenRen.Systems;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class RecipeSongYouFeng : AbstractRecipeRelic
{
    private static readonly IReadOnlyList<RecipeIngredient> RequiredIngredients =
    [
        new(
            "ASSEMBLE_SONG_YOU_FENG_BA_MIAN",
            static card => card is BaMianWeiFengGu { IsUpgraded: true }),
        new(
            "ASSEMBLE_SONG_YOU_FENG_XIAN_GU",
            static card =>
                card is GuZhenRenCardTemplate { Rank: >= 1 and <= 9 }
                && (GuZhenRenTagRules.HasEffectiveTag(
                        card,
                        GuZhenRenTags.FengDao)
                    || GuZhenRenTagRules.HasEffectiveTag(
                        card,
                        GuZhenRenTags.TouDao))
        )
    ];

    internal override CardModel RewardCard => ModelDb.Card<SongYouFeng>();

    internal override IReadOnlyList<RecipeIngredient> Ingredients =>
        RequiredIngredients;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/relics/Recipe_FengDao.png",
        IconOutlinePath:
            "res://GuZhenRen/images/relics/outline/Recipe_FengDao.png",
        BigIconPath: "res://GuZhenRen/images/relics/Recipe_FengDao.png");
}
