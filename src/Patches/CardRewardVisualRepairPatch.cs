using Godot;
using MegaCrit.Sts2.Core.Entities.CardRewardAlternatives;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using STS2RitsuLib.Patching.Models;

namespace GuZhenRen.Patches;

public sealed class CardRewardVisualRepairPatch : IPatchMethod
{
    public static string PatchId => "card-reward-visual-repair";

    public static string Description =>
        "Restore card reward visuals removed by deferred pooled-node cleanup";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(
            typeof(NCardRewardSelectionScreen),
            nameof(NCardRewardSelectionScreen.RefreshOptions),
            [
                typeof(IReadOnlyList<CardCreationResult>),
                typeof(IReadOnlyList<CardRewardAlternative>)
            ])
    ];

    public static void Postfix(
        NCardRewardSelectionScreen __instance,
        IReadOnlyList<CardCreationResult> options,
        IReadOnlyList<CardRewardAlternative> extraOptions)
    {
        Callable.From(() => RepairAfterDeferredCleanup(
                __instance,
                options,
                extraOptions))
            .CallDeferred();
    }

    private static void RepairAfterDeferredCleanup(
        NCardRewardSelectionScreen screen,
        IReadOnlyList<CardCreationResult> options,
        IReadOnlyList<CardRewardAlternative> extraOptions)
    {
        if (!GodotObject.IsInstanceValid(screen)
            || screen.IsQueuedForDeletion()
            || !screen.IsInsideTree())
        {
            return;
        }

        var holders = screen
            .GetNode<Control>("UI/CardRow")
            .GetChildren()
            .OfType<NGridCardHolder>()
            .ToList();
        if (holders.Count != options.Count
            || holders.Any(static holder =>
                holder.CardNode is null
                || !GodotObject.IsInstanceValid(holder.CardNode)
                || holder.CardNode.IsQueuedForDeletion()
                || holder.CardNode.GetParent() != holder))
        {
            Entry.Logger.Warn(
                $"Rebuilding card reward visuals after a card node was removed. "
                + $"Rewards: {DescribeRewards(options)}");
            screen.RefreshOptions(options, extraOptions);
            return;
        }

        for (var index = 0; index < holders.Count; index++)
        {
            var holder = holders[index];
            var cardNode = holder.CardNode!;
            if (holder.Visible
                && holder.Modulate.A > 0f
                && cardNode.Visible
                && cardNode.Modulate.A > 0f
                && cardNode.Scale.X > 0.01f
                && cardNode.Scale.Y > 0.01f
                && cardNode.Body.Visible
                && cardNode.Body.Modulate.A > 0f)
            {
                continue;
            }

            Entry.Logger.Warn(
                $"Restoring hidden card reward visual at index {index}: "
                + DescribeReward(options[index]));
            holder.Visible = true;
            holder.Modulate = Colors.White;
            cardNode.Visible = true;
            cardNode.Modulate = Colors.White;
            cardNode.Scale = Vector2.One;
            cardNode.Body.Visible = true;
            cardNode.Body.Modulate = Colors.White;
            cardNode.UpdateVisuals(PileType.None, CardPreviewMode.Normal);
        }
    }

    private static string DescribeRewards(
        IEnumerable<CardCreationResult> options) =>
        string.Join(", ", options.Select(DescribeReward));

    private static string DescribeReward(CardCreationResult result) =>
        $"{result.Card.Id.Entry}{(result.Card.IsUpgraded ? "+" : string.Empty)}";
}
