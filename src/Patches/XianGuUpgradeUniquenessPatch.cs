using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using STS2RitsuLib.Patching.Models;

namespace GuZhenRen.Patches;

public sealed class XianGuUpgradeUniquenessPatch : IPatchMethod
{
    public static string PatchId => "xian-gu-upgrade-uniqueness";

    public static string Description =>
        "Enforces XianGu uniqueness when a deck card is upgraded into rank six or higher.";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(
            typeof(CardCmd),
            nameof(CardCmd.Upgrade),
            [typeof(IEnumerable<CardModel>), typeof(CardPreviewStyle)])
    ];

    public static void Prefix(
        ref IEnumerable<CardModel> cards,
        out UpgradeState __state)
    {
        var materializedCards = cards.ToList();
        cards = materializedCards;

        var owners = materializedCards
            .Where(static card => card.Pile?.Type == PileType.Deck)
            .Select(static card => card.Owner)
            .Distinct()
            .ToList();

        __state = new UpgradeState(owners);
    }

    public static void Postfix(UpgradeState __state)
    {
        foreach (var owner in __state.Owners)
        {
            TaskHelper.RunSafely(
                BenMingGuUniquenessPatch.EnforceDeckUniqueness(owner));
        }
    }

    public sealed record UpgradeState(
        IReadOnlyList<Player> Owners);
}
