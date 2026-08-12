using GuZhenRen.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Patching.Models;

namespace GuZhenRen.Patches;

public sealed class CardHoverTipFilterPatch : IPatchMethod
{
    public static string PatchId => "card_hover_tip_filter";

    public static string Description =>
        "Only show power tooltips referenced by the current card text";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(
            typeof(CardModel),
            "get_HoverTips",
            Type.EmptyTypes)
    ];

    public static void Postfix(
        CardModel __instance,
        ref IEnumerable<IHoverTip> __result)
    {
        if (__instance is not GuZhenRenCardTemplate)
        {
            return;
        }

        var description = __instance.GetDescriptionForPile(
            __instance.Pile?.Type ?? PileType.None);
        __result = __result
            .Where(tip => ShouldKeep(tip, description))
            .ToList();
    }

    private static bool ShouldKeep(IHoverTip tip, string description)
    {
        var power = ModelDb.AllPowers.FirstOrDefault(candidate =>
            string.Equals(
                candidate.Id.ToString(),
                tip.Id,
                StringComparison.Ordinal));
        if (power is null)
        {
            return true;
        }

        var title = power.Title.GetFormattedText();
        return !string.IsNullOrWhiteSpace(title)
            && description.Contains(title, StringComparison.Ordinal);
    }
}
