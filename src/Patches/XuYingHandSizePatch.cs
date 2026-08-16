using GuZhenRen.Cards;
using GuZhenRen.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Patching.Models;

namespace GuZhenRen.Patches;

public sealed class XuYingHandSizePatch : IPatchMethod
{
    private static readonly AsyncLocal<Dictionary<Player, int>?> PendingAllowances = new();

    public static string PatchId => "xu-ying-hand-size";

    public static string Description =>
        "Allows XuYing cards to enter a full hand while LiQi is active.";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(
            typeof(CardPileCmd),
            nameof(CardPileCmd.Add),
            [
                typeof(IEnumerable<CardModel>),
                typeof(CardPile),
                typeof(CardPilePosition),
                typeof(AbstractModel),
                typeof(bool),
                typeof(bool)
            ])
    ];

    public static void Prefix(
        ref IEnumerable<CardModel> cards,
        CardPile newPile,
        out Dictionary<Player, int>? __state)
    {
        __state = PendingAllowances.Value;
        if (newPile.Type != PileType.Hand)
        {
            return;
        }

        var materializedCards = cards.ToList();
        cards = materializedCards;

        var additions = materializedCards
            .Where(static card =>
                card is AbstractXuYingCard
                && card.Owner.Creature.GetPower<LiQiPower>() is not null)
            .GroupBy(static card => card.Owner)
            .Select(static group => (Player: group.Key, Count: group.Count()))
            .ToList();

        if (additions.Count == 0)
        {
            return;
        }

        var next = __state is null
            ? []
            : new Dictionary<Player, int>(__state);

        foreach (var (player, count) in additions)
        {
            next[player] = next.GetValueOrDefault(player) + count;
        }

        PendingAllowances.Value = next;
    }

    public static void Postfix(Dictionary<Player, int>? __state)
    {
        PendingAllowances.Value = __state;
    }

    internal static int GetPendingAllowance(Player player) =>
        PendingAllowances.Value?.GetValueOrDefault(player) ?? 0;
}
