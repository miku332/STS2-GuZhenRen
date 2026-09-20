using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using STS2RitsuLib.Patching.Models;

namespace GuZhenRen.Patches;

public sealed class XuYingTriggerCardVisualPatch : IPatchMethod
{
    private static readonly HashSet<CardModel> PendingCards =
        new(ReferenceEqualityComparer.Instance);

    public static string PatchId => "xu-ying-trigger-card-visual";

    public static string Description =>
        "Skips the result-pile animation for attacks that trigger XuYing cards.";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(
            typeof(CardPileCmd),
            nameof(CardPileCmd.Add),
            [
                typeof(CardModel),
                typeof(CardPile),
                typeof(CardPilePosition),
                typeof(AbstractModel),
                typeof(bool)
            ])
    ];

    internal static void Mark(CardModel card)
    {
        if (LocalContext.IsMine(card))
        {
            PendingCards.Add(card);
        }
    }

    public static void Prefix(
        CardModel card,
        CardPile newPile,
        ref bool skipVisuals,
        out PendingVisual? __state)
    {
        __state = null;
        if (card.Pile?.Type != PileType.Play
            || newPile.Type == PileType.Play
            || !PendingCards.Remove(card))
        {
            return;
        }

        __state = new PendingVisual(card, NCard.FindOnTable(card));
        skipVisuals = true;
    }

    public static void Postfix(
        PendingVisual? __state,
        ref Task<CardPileAddResult> __result)
    {
        if (__state is not null)
        {
            __result = ReleaseCardNodeAfterMove(__result, __state);
        }
    }

    internal static void Clear() => PendingCards.Clear();

    private static async Task<CardPileAddResult> ReleaseCardNodeAfterMove(
        Task<CardPileAddResult> original,
        PendingVisual state)
    {
        try
        {
            return await original;
        }
        finally
        {
            if (state.CardNode is { } cardNode
                && GodotObject.IsInstanceValid(cardNode)
                && !cardNode.IsQueuedForDeletion()
                && ReferenceEquals(cardNode.Model, state.Card))
            {
                cardNode.QueueFreeSafely();
            }
        }
    }

    public sealed record PendingVisual(CardModel Card, NCard? CardNode);
}
