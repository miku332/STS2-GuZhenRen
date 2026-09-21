using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2RitsuLib.Patching.Models;

namespace GuZhenRen.Patches;

public sealed class XuYingTriggerCardVisualPatch : IPatchMethod
{
    private static readonly HashSet<CardModel> PendingCards =
        new(ReferenceEqualityComparer.Instance);
    private static readonly HashSet<NCard> DetachedCardNodes = [];

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
        ref bool skipVisuals)
    {
        if (card.Pile?.Type != PileType.Play
            || newPile.Type == PileType.Play
            || !PendingCards.Remove(card))
        {
            return;
        }

        DetachCardNode(card);
        skipVisuals = true;
    }

    internal static void Clear()
    {
        PendingCards.Clear();

        foreach (var cardNode in DetachedCardNodes)
        {
            if (GodotObject.IsInstanceValid(cardNode)
                && !cardNode.IsQueuedForDeletion())
            {
                cardNode.QueueFreeSafely();
            }
        }

        DetachedCardNodes.Clear();
    }

    private static void DetachCardNode(CardModel card)
    {
        var combatUi = NCombatRoom.Instance?.Ui;
        if (combatUi is null)
        {
            return;
        }

        var cardNode = combatUi.GetCardFromPlayContainer(card)
            ?? combatUi.PlayQueue.GetCardNode(card);
        if (cardNode is null
            || !GodotObject.IsInstanceValid(cardNode)
            || cardNode.IsQueuedForDeletion())
        {
            return;
        }

        if (combatUi.PlayQueue.GetCardNode(card) is not null)
        {
            combatUi.PlayQueue.RemoveCardFromQueueForExecution(card);
        }

        cardNode.GetParent()?.RemoveChildSafely(cardNode);
        cardNode.Visible = false;
        DetachedCardNodes.Add(cardNode);
    }
}
