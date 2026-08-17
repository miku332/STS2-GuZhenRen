using System.Reflection;
using System.Runtime.CompilerServices;
using Godot;
using GuZhenRen.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Relics;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Patching.Models;

namespace GuZhenRen.Patches;

public sealed class KillerMoveRelicRowPatch : IPatchMethod
{
    private const string RecipeRowName = "GuZhenRenKillerMoveRelicRow";

    private static readonly ConditionalWeakTable<NRelicInventory, object>
        ResizeSubscriptions = new();

    private static readonly MethodInfo? UpdateNavigationMethod =
        typeof(NRelicInventory).GetMethod(
            "UpdateNavigation",
            BindingFlags.Instance | BindingFlags.NonPublic);

    public static string PatchId => "killer-move-relic-row";

    public static string Description =>
        "Displays killer move recipe relics on a dedicated row.";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(
            typeof(NRelicInventory),
            nameof(NRelicInventory.Initialize),
            [typeof(RunState)]),
        new ModPatchTarget(
            typeof(NRelicInventory),
            "OnRelicObtained",
            [typeof(RelicModel)]),
        new ModPatchTarget(
            typeof(NRelicInventory),
            "OnRelicRemoved",
            [typeof(RelicModel)])
    ];

    public static void Prefix(NRelicInventory __instance)
    {
        RestoreVanillaLayout(__instance);
    }

    public static void Postfix(NRelicInventory __instance)
    {
        ApplyRecipeLayout(__instance);
    }

    private static void RestoreVanillaLayout(NRelicInventory inventory)
    {
        var row = FindRecipeRow(inventory);
        if (row is null)
        {
            return;
        }

        foreach (var holder in inventory.RelicNodes)
        {
            if (holder.GetParent() != inventory)
            {
                holder.Reparent(inventory, keepGlobalTransform: false);
            }
        }

        inventory.RemoveChild(row);
        row.QueueFree();

        for (var i = 0; i < inventory.RelicNodes.Count; i++)
        {
            inventory.MoveChild(inventory.RelicNodes[i], i);
        }
    }

    private static void ApplyRecipeLayout(NRelicInventory inventory)
    {
        var recipeHolders = inventory.RelicNodes
            .Where(static holder =>
                holder.Relic.Model is AbstractRecipeRelic)
            .ToList();
        if (recipeHolders.Count == 0)
        {
            UpdateNavigationMethod?.Invoke(inventory, null);
            return;
        }

        var normalHolders = inventory.RelicNodes
            .Where(static holder =>
                holder.Relic.Model is not AbstractRecipeRelic)
            .ToList();
        var row = new HFlowContainer
        {
            Name = RecipeRowName,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ShrinkBegin
        };
        row.AddThemeConstantOverride(
            "h_separation",
            inventory.GetThemeConstant("h_separation"));
        row.AddThemeConstantOverride(
            "v_separation",
            inventory.GetThemeConstant("v_separation"));

        inventory.AddChild(row);
        foreach (var holder in recipeHolders)
        {
            holder.Reparent(row, keepGlobalTransform: false);
        }

        for (var i = 0; i < normalHolders.Count; i++)
        {
            inventory.MoveChild(normalHolders[i], i);
        }

        inventory.MoveChild(row, normalHolders.Count);
        for (var i = 0; i < recipeHolders.Count; i++)
        {
            row.MoveChild(recipeHolders[i], i);
        }

        EnsureResizeSubscription(inventory);
        UpdateRecipeRowWidth(inventory);
        Callable.From(() => UpdateRecipeRowWidth(inventory)).CallDeferred();
        UpdateNavigationMethod?.Invoke(inventory, null);
    }

    private static void EnsureResizeSubscription(NRelicInventory inventory)
    {
        if (ResizeSubscriptions.TryGetValue(inventory, out _))
        {
            return;
        }

        ResizeSubscriptions.Add(inventory, new object());
        inventory.Resized += () => UpdateRecipeRowWidth(inventory);
    }

    private static void UpdateRecipeRowWidth(NRelicInventory inventory)
    {
        if (!GodotObject.IsInstanceValid(inventory)
            || FindRecipeRow(inventory) is not { } row)
        {
            return;
        }

        var availableWidth = inventory.Size.X;
        if (availableWidth <= 0f
            && inventory.GetParent() is Control parent)
        {
            availableWidth = parent.Size.X;
        }

        if (availableWidth > 0f)
        {
            row.CustomMinimumSize = new Vector2(availableWidth, 0f);
        }
    }

    private static HFlowContainer? FindRecipeRow(NRelicInventory inventory) =>
        inventory.GetNodeOrNull<HFlowContainer>(RecipeRowName);
}
