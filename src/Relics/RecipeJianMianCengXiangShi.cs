using GuZhenRen.CardPools;
using GuZhenRen.Cards;
using GuZhenRen.Systems;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class RecipeJianMianCengXiangShi : AbstractRecipeRelic
{
    private static readonly IReadOnlyList<RecipeIngredient> RequiredIngredients =
    [
        new(
            "ASSEMBLE_JIAN_MIAN_CENG_XIANG_SHI_TAI_DU",
            static card => card is TaiDuGu { IsUpgraded: true }),
        new(
            "ASSEMBLE_JIAN_MIAN_CENG_XIANG_SHI_XIAN_GU",
            static card =>
                card is GuZhenRenCardTemplate { Rank: >= 1 and <= 9 }
                && (GuZhenRenTagRules.HasEffectiveTag(
                        card,
                        GuZhenRenTags.BianHuaDao)
                    || GuZhenRenTagRules.HasEffectiveTag(
                        card,
                        GuZhenRenTags.LuDao))
        )
    ];

    internal override CardModel RewardCard =>
        ModelDb.Card<JianMianCengXiangShi>();

    internal override IReadOnlyList<RecipeIngredient> Ingredients =>
        RequiredIngredients;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/relics/Recipe_BianHuaDao.png",
        IconOutlinePath:
            "res://GuZhenRen/images/relics/outline/Recipe_BianHuaDao.png",
        BigIconPath: "res://GuZhenRen/images/relics/Recipe_BianHuaDao.png");
}
