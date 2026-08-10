using System.Reflection;
using GuZhenRen.Cards;
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
        if (rank <= 0)
        {
            return;
        }

        var rankText = new LocString(
                "card_keywords",
                $"GU_ZHEN_REN_KEYWORD_PIN_JIE_{rank}.title")
            .GetFormattedText();
        if (string.IsNullOrWhiteSpace(rankText))
        {
            return;
        }

        __result = $"[gold]{rankText}[/gold]\n{__result}";
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
