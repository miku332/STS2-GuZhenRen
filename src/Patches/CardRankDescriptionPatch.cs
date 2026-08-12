using System.Reflection;
using GuZhenRen.Cards;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Patching.Models;

namespace GuZhenRen.Patches;

public sealed class CardRankDescriptionPatch : IPatchMethod
{
    private static readonly Type DescriptionPreviewType =
        typeof(CardModel).GetNestedType(
            "DescriptionPreviewType",
            BindingFlags.NonPublic)
        ?? throw new MissingMemberException(
            typeof(CardModel).FullName,
            "DescriptionPreviewType");

    public static string PatchId => "card_rank_description";

    public static string Description =>
        "Show Gu card rank before the base card description";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(
            typeof(CardModel),
            nameof(CardModel.GetDescriptionForPile),
            [typeof(PileType), DescriptionPreviewType, typeof(Creature)])
    ];

    public static void Postfix(
        CardModel __instance,
        object[] __args,
        ref string __result)
    {
        if (__instance is not GuZhenRenCardTemplate card)
        {
            return;
        }

        var isUpgradePreview = __args.Length > 1
            && string.Equals(
                __args[1]?.ToString(),
                "Upgrade",
                StringComparison.Ordinal);
        var rank = ResolveDisplayedRank(card, isUpgradePreview);
        var rankText = rank > 0
            ? new LocString(
                    "card_keywords",
                    $"GU_ZHEN_REN_KEYWORD_PIN_JIE_{rank}.title")
                .GetFormattedText()
            : string.Empty;

        var benMingGuText = card is AbstractBenMingGuCard
            ? new LocString(
                    "card_keywords",
                    "GU_ZHEN_REN_KEYWORD_BEN_MING_GU_TYPE.title")
                .GetFormattedText()
            : string.Empty;

        var xianGuText = rank >= 6
            && card is not AbstractShaZhaoCard
            && card is not AbstractBenMingGuCard
            ? new LocString(
                    "card_keywords",
                    "GU_ZHEN_REN_KEYWORD_XIAN_GU_TYPE.title")
                .GetFormattedText()
            : string.Empty;

        var shaZhaoText = card is AbstractShaZhaoCard
            ? new LocString(
                    "card_keywords",
                    "GU_ZHEN_REN_KEYWORD_SHA_ZHAO.title")
                .GetFormattedText()
            : string.Empty;

        var daoTexts = GuZhenRenTagRules.GetEffectiveDaoTags(card)
            .Select(GuZhenRenCardTemplate.GetDaoKeywordStem)
            .Where(static stem => stem is not null)
            .Select(static stem => new LocString(
                "card_keywords",
                $"GU_ZHEN_REN_KEYWORD_{stem}.title").GetFormattedText())
            .Where(static text => !string.IsNullOrWhiteSpace(text))
            .ToList();
        if (string.IsNullOrWhiteSpace(shaZhaoText)
            && string.IsNullOrWhiteSpace(rankText)
            && daoTexts.Count == 0)
        {
            return;
        }

        var rankAndDaoText = string.Join(
            " ",
            new[] { shaZhaoText, rankText }
                .Where(static text => !string.IsNullOrWhiteSpace(text))
                .Concat(daoTexts)
                .Concat(new[] { benMingGuText, xianGuText }
                    .Where(static text => !string.IsNullOrWhiteSpace(text))));

        __result = $"[gold]{rankAndDaoText}[/gold]\n{__result}";
    }

    private static int ResolveDisplayedRank(
        GuZhenRenCardTemplate card,
        bool isUpgradePreview)
    {
        if (!isUpgradePreview || !card.IsUpgradable)
        {
            return card.Rank;
        }

        var preview = (GuZhenRenCardTemplate)card.MutableClone();
        preview.UpgradeInternal();
        preview.FinalizeUpgradeInternal();
        return preview.Rank;
    }
}
