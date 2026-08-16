using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.Tags;

namespace GuZhenRen.Cards;

public abstract class GuZhenRenCardTemplate : ModCardTemplate
{
    private readonly bool _upgrades;

    private static readonly IHoverTip XianGuHoverTip = new HoverTip(
        new LocString(
            "card_keywords",
            "GU_ZHEN_REN_KEYWORD_XIAN_GU.title"),
        new LocString(
            "card_keywords",
            "GU_ZHEN_REN_KEYWORD_XIAN_GU.description"));

    private static readonly IHoverTip XianGuWuHoverTip = new HoverTip(
        new LocString(
            "card_keywords",
            "GU_ZHEN_REN_KEYWORD_XIAN_GU_WU.title"),
        new LocString(
            "card_keywords",
            "GU_ZHEN_REN_KEYWORD_XIAN_GU_WU.description"));

    private static readonly IHoverTip FanGuWuHoverTip = new HoverTip(
        new LocString(
            "card_keywords",
            "GU_ZHEN_REN_KEYWORD_FAN_GU_WU.title"),
        new LocString(
            "card_keywords",
            "GU_ZHEN_REN_KEYWORD_FAN_GU_WU.description"));

    private static readonly IHoverTip BenMingGuHoverTip = new HoverTip(
        new LocString(
            "card_keywords",
            "GU_ZHEN_REN_KEYWORD_BEN_MING_GU_TYPE.title"),
        new LocString(
            "card_keywords",
            "GU_ZHEN_REN_KEYWORD_BEN_MING_GU_TYPE.description"));

    private static readonly IHoverTip ShaZhaoHoverTip = new HoverTip(
        new LocString(
            "card_keywords",
            "GU_ZHEN_REN_KEYWORD_SHA_ZHAO.title"),
        new LocString(
            "card_keywords",
            "GU_ZHEN_REN_KEYWORD_SHA_ZHAO.description"));

    private static readonly IHoverTip XueKuangHoverTip = new HoverTip(
        new LocString(
            "card_keywords",
            "GU_ZHEN_REN_KEYWORD_XUE_KUANG.title"),
        new LocString(
            "card_keywords",
            "GU_ZHEN_REN_KEYWORD_XUE_KUANG.description"));

    protected readonly record struct GeneratedCardPreview(
        CardModel Card,
        bool Upgraded);

    public virtual int Rank => 1;

    protected virtual bool ShowXianGuHoverTip => true;

    public override int MaxUpgradeLevel => _upgrades ? base.MaxUpgradeLevel : 0;

    protected virtual IEnumerable<GeneratedCardPreview> GeneratedCardPreviews =>
        [];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            if (Rank > 0)
            {
                yield return CreateRankHoverTip(Rank);
            }

            foreach (var daoTag in GuZhenRenTagRules.GetEffectiveDaoTags(this))
            {
                var hoverTip = CreateDaoHoverTip(daoTag);
                if (hoverTip is not null)
                {
                    yield return hoverTip;
                }
            }

            if (Rank >= 6
                && ShowXianGuHoverTip
                && this is not AbstractShaZhaoCard
                && this is not AbstractBenMingGuCard)
            {
                yield return XianGuHoverTip;
            }

            if (this is AbstractBenMingGuCard)
            {
                yield return BenMingGuHoverTip;
            }

            if (this is AbstractShaZhaoCard)
            {
                yield return ShaZhaoHoverTip;
            }

            if (Tags.Contains(GuZhenRenTags.XianGuWu))
            {
                yield return XianGuWuHoverTip;
            }

            if (Tags.Contains(GuZhenRenTags.FanGuWu))
            {
                yield return FanGuWuHoverTip;
            }

            if (this is XueKuangGu)
            {
                yield return XueKuangHoverTip;
            }

            foreach (var preview in GeneratedCardPreviews)
            {
                yield return HoverTipFactory.FromCard(
                    preview.Card,
                    preview.Upgraded);
            }
        }
    }

    private static IHoverTip? CreateDaoHoverTip(CardTag tag)
    {
        var keywordStem = GetDaoKeywordStem(tag);
        return keywordStem is null
            ? null
            : new HoverTip(
                new LocString(
                    "card_keywords",
                    $"GU_ZHEN_REN_KEYWORD_{keywordStem}.title"),
                new LocString(
                    "card_keywords",
                    $"GU_ZHEN_REN_KEYWORD_{keywordStem}.description"));
    }

    private static IHoverTip CreateRankHoverTip(int rank) => new HoverTip(
        new LocString(
            "card_keywords",
            $"GU_ZHEN_REN_KEYWORD_PIN_JIE_{rank}.title"),
        new LocString(
            "card_keywords",
            "GU_ZHEN_REN_KEYWORD_PIN_JIE.description"));

    internal static string? GetDaoKeywordStem(CardTag tag) =>
        tag == GuZhenRenTags.BianHuaDao ? "BIAN_HUA_DAO"
            : tag == GuZhenRenTags.FengDao ? "FENG_DAO"
            : tag == GuZhenRenTags.GuangDao ? "GUANG_DAO"
            : tag == GuZhenRenTags.GuDao ? "GU_DAO"
            : tag == GuZhenRenTags.GuChongDao ? "GU_CHONG_DAO"
            : tag == GuZhenRenTags.JianDao ? "JIAN_DAO"
            : tag == GuZhenRenTags.JinDao ? "JIN_DAO"
            : tag == GuZhenRenTags.LiDao ? "LI_DAO"
            : tag == GuZhenRenTags.LuDao ? "LU_DAO"
            : tag == GuZhenRenTags.MuDao ? "MU_DAO"
            : tag == GuZhenRenTags.ShaDao ? "SHA_DAO"
            : tag == GuZhenRenTags.ShiDao ? "SHI_DAO"
            : tag == GuZhenRenTags.TuDao ? "TU_DAO"
            : tag == GuZhenRenTags.TouDao ? "TOU_DAO"
            : tag == GuZhenRenTags.YanDao ? "YAN_DAO"
            : tag == GuZhenRenTags.XueDao ? "XUE_DAO"
            : tag == GuZhenRenTags.ZhiDao ? "ZHI_DAO"
            : tag == GuZhenRenTags.YunDao ? "YUN_DAO"
            : tag == GuZhenRenTags.ZhouDao ? "ZHOU_DAO"
            : null;

    protected GuZhenRenCardTemplate(
        int energyCost,
        CardType cardType,
        CardRarity rarity,
        TargetType targetType,
        bool upgrades)
        : base(energyCost, cardType, rarity, targetType)
    {
        _upgrades = upgrades;
    }

    protected static GeneratedCardPreview PreviewCard<T>(
        bool upgraded = false)
        where T : CardModel =>
        new(ModelDb.Card<T>(), upgraded);
}
