using System.Reflection;
using System.Runtime.CompilerServices;
using Godot;
using GuZhenRen.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Relics;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Patching.Models;

namespace GuZhenRen.Patches;

public sealed class KillerMoveRelicRowPatch : IPatchMethod
{
    private const string NormalRowName = "GuZhenRenNormalRelicRow";
    private const string RecipeRowName = "GuZhenRenKillerMoveRelicRow";
    private const string ContentName = "Content";
    private const string LeftArrowName = "LeftArrow";
    private const string RightArrowName = "RightArrow";
    private const float ArrowWidth = 48f;
    private const float FallbackHolderWidth = 68f;
    private const float FallbackHolderHeight = 68f;

    private static readonly ConditionalWeakTable<NRelicInventory, PagingState> PagingStates = new();
    private static readonly ConditionalWeakTable<NRelicInventory, object> ResizeSubscriptions = new();

    private static readonly MethodInfo? UpdateNavigationMethod =
        typeof(NRelicInventory).GetMethod(
            "UpdateNavigation",
            BindingFlags.Instance | BindingFlags.NonPublic);

    public static string PatchId => "killer-move-relic-row";

    public static string Description =>
        "Displays normal and killer move relics in separate paged rows.";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(typeof(NRelicInventory), nameof(NRelicInventory.Initialize), [typeof(RunState)]),
        new ModPatchTarget(typeof(NRelicInventory), "OnRelicObtained", [typeof(RelicModel)]),
        new ModPatchTarget(typeof(NRelicInventory), "OnRelicRemoved", [typeof(RelicModel)])
    ];

    public static void Prefix(NRelicInventory __instance)
    {
        RestoreVanillaLayout(__instance);
    }

    public static void Postfix(NRelicInventory __instance)
    {
        ApplyPagedLayout(__instance);
    }

    private static void RestoreVanillaLayout(NRelicInventory inventory)
    {
        var rows = inventory.GetChildren()
            .OfType<HBoxContainer>()
            .Where(static child => child.Name == NormalRowName || child.Name == RecipeRowName)
            .ToList();
        if (rows.Count == 0)
        {
            return;
        }

        foreach (var holder in inventory.RelicNodes)
        {
            if (holder.GetParent() != inventory)
            {
                holder.Reparent(inventory, keepGlobalTransform: false);
            }
            holder.Visible = true;
            holder.FocusMode = Control.FocusModeEnum.All;
        }

        foreach (var row in rows)
        {
            inventory.RemoveChild(row);
            row.QueueFree();
        }

        for (var i = 0; i < inventory.RelicNodes.Count; i++)
        {
            inventory.MoveChild(inventory.RelicNodes[i], i);
        }
    }

    private static void ApplyPagedLayout(NRelicInventory inventory)
    {
        var normalHolders = inventory.RelicNodes
            .Where(static holder => holder.Relic.Model is not AbstractRecipeRelic)
            .ToList();
        var recipeHolders = inventory.RelicNodes
            .Where(static holder => holder.Relic.Model is AbstractRecipeRelic)
            .ToList();

        if (normalHolders.Count > 0)
        {
            CreateRow(inventory, NormalRowName, normalHolders, recipeRow: false);
        }
        if (recipeHolders.Count > 0)
        {
            CreateRow(inventory, RecipeRowName, recipeHolders, recipeRow: true);
        }

        EnsureResizeSubscription(inventory);
        UpdateRows(inventory);
        Callable.From(() => UpdateRows(inventory)).CallDeferred();
        UpdateNavigation(inventory);
    }

    private static void CreateRow(
        NRelicInventory inventory,
        string rowName,
        IReadOnlyList<NRelicInventoryHolder> holders,
        bool recipeRow)
    {
        var row = new HBoxContainer
        {
            Name = rowName,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ShrinkBegin
        };
        row.AddThemeConstantOverride("separation", inventory.GetThemeConstant("h_separation"));

        var leftArrow = CreateArrow(inventory, recipeRow, left: true);
        var content = new HFlowContainer
        {
            Name = ContentName,
            ClipContents = true,
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            SizeFlagsVertical = Control.SizeFlags.ShrinkCenter,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        var rightArrow = CreateArrow(inventory, recipeRow, left: false);

        row.AddChild(leftArrow);
        row.AddChild(content);
        row.AddChild(rightArrow);
        inventory.AddChild(row);

        foreach (var holder in holders)
        {
            holder.Reparent(content, keepGlobalTransform: false);
        }

        var rowIndex = recipeRow && FindRow(inventory, NormalRowName) is not null ? 1 : 0;
        inventory.MoveChild(row, rowIndex);
    }

    private static KillerMovePageArrow CreateArrow(
        NRelicInventory inventory,
        bool recipeRow,
        bool left)
    {
        var arrow = new KillerMovePageArrow(left)
        {
            Name = left ? LeftArrowName : RightArrowName,
            CustomMinimumSize = new Vector2(ArrowWidth, FallbackHolderHeight),
            SizeFlagsVertical = Control.SizeFlags.ShrinkCenter,
            MouseFilter = Control.MouseFilterEnum.Stop
        };
        arrow.Connect(
            NClickableControl.SignalName.Released,
            Callable.From<NClickableControl>(_ => ChangePage(inventory, recipeRow, left ? -1 : 1)));
        return arrow;
    }

    private static void ChangePage(NRelicInventory inventory, bool recipeRow, int delta)
    {
        var state = PagingStates.GetOrCreateValue(inventory);
        if (recipeRow)
        {
            state.RecipePage += delta;
        }
        else
        {
            state.NormalPage += delta;
        }

        UpdateRows(inventory);
        UpdateNavigation(inventory);
    }

    private static void EnsureResizeSubscription(NRelicInventory inventory)
    {
        if (ResizeSubscriptions.TryGetValue(inventory, out _))
        {
            return;
        }

        ResizeSubscriptions.Add(inventory, new object());
        inventory.Resized += () => UpdateRows(inventory);
    }

    private static void UpdateRows(NRelicInventory inventory)
    {
        if (!GodotObject.IsInstanceValid(inventory))
        {
            return;
        }

        var availableWidth = GetAvailableWidth(inventory);
        UpdateRow(inventory, NormalRowName, availableWidth, recipeRow: false);
        UpdateRow(inventory, RecipeRowName, availableWidth, recipeRow: true);
    }

    private static void UpdateRow(
        NRelicInventory inventory,
        string rowName,
        float availableWidth,
        bool recipeRow)
    {
        if (FindRow(inventory, rowName) is not { } row
            || row.GetNodeOrNull<HFlowContainer>(ContentName) is not { } content)
        {
            return;
        }

        var holders = inventory.RelicNodes
            .Where(holder => holder.Relic.Model is AbstractRecipeRelic == recipeRow)
            .ToList();
        if (holders.Count == 0)
        {
            return;
        }

        var state = PagingStates.GetOrCreateValue(inventory);
        var holderWidth = holders
            .Select(GetHolderWidth)
            .Where(static width => width > 1f)
            .DefaultIfEmpty(FallbackHolderWidth)
            .Max();
        var separation = inventory.GetThemeConstant("h_separation");
        var pageSizeWithoutArrows = GetPageSize(availableWidth, holderWidth, separation);
        var paged = holders.Count > pageSizeWithoutArrows;
        var contentWidth = paged
            ? Mathf.Max(1f, availableWidth - ArrowWidth * 2f - separation * 2f)
            : availableWidth;
        var pageSize = GetPageSize(contentWidth, holderWidth, separation);
        var maxPage = GetMaxPage(holders.Count, pageSize);
        var page = recipeRow ? state.RecipePage : state.NormalPage;
        page = Mathf.Clamp(page, 0, maxPage);
        if (recipeRow)
        {
            state.RecipePage = page;
        }
        else
        {
            state.NormalPage = page;
        }

        row.CustomMinimumSize = new Vector2(availableWidth, Mathf.Max(FallbackHolderHeight, GetHolderHeight(holders)));
        content.CustomMinimumSize = new Vector2(contentWidth, 0f);
        var leftArrow = row.GetNode<KillerMovePageArrow>(LeftArrowName);
        var rightArrow = row.GetNode<KillerMovePageArrow>(RightArrowName);
        leftArrow.Visible = paged;
        rightArrow.Visible = paged;
        leftArrow.SetEnabled(page > 0);
        rightArrow.SetEnabled(page < maxPage);

        for (var i = 0; i < holders.Count; i++)
        {
            var visible = i >= page * pageSize && i < (page + 1) * pageSize;
            holders[i].Visible = visible;
            holders[i].FocusMode = visible
                ? Control.FocusModeEnum.All
                : Control.FocusModeEnum.None;
        }
    }

    private static void UpdateNavigation(NRelicInventory inventory)
    {
        UpdateNavigationMethod?.Invoke(inventory, null);
        SetRowNavigation(inventory, NormalRowName);
        SetRowNavigation(inventory, RecipeRowName);
    }

    private static void SetRowNavigation(NRelicInventory inventory, string rowName)
    {
        if (FindRow(inventory, rowName) is not { } row
            || row.GetNodeOrNull<HFlowContainer>(ContentName) is not { } content)
        {
            return;
        }

        var visibleHolders = content.GetChildren()
            .OfType<NRelicInventoryHolder>()
            .Where(static holder => holder.Visible)
            .ToList();
        if (visibleHolders.Count == 0)
        {
            return;
        }

        var leftArrow = row.GetNode<KillerMovePageArrow>(LeftArrowName);
        var rightArrow = row.GetNode<KillerMovePageArrow>(RightArrowName);
        var firstPath = visibleHolders[0].GetPath();
        var lastPath = visibleHolders[^1].GetPath();

        for (var i = 0; i < visibleHolders.Count; i++)
        {
            var holder = visibleHolders[i];
            holder.FocusNeighborLeft = i == 0 && leftArrow.Visible
                ? leftArrow.GetPath()
                : visibleHolders[(i + visibleHolders.Count - 1) % visibleHolders.Count].GetPath();
            holder.FocusNeighborRight = i == visibleHolders.Count - 1 && rightArrow.Visible
                ? rightArrow.GetPath()
                : visibleHolders[(i + 1) % visibleHolders.Count].GetPath();
        }

        leftArrow.FocusNeighborRight = firstPath;
        rightArrow.FocusNeighborLeft = lastPath;
        leftArrow.FocusNeighborTop = visibleHolders[0].FocusNeighborTop;
        leftArrow.FocusNeighborBottom = visibleHolders[0].FocusNeighborBottom;
        rightArrow.FocusNeighborTop = visibleHolders[^1].FocusNeighborTop;
        rightArrow.FocusNeighborBottom = visibleHolders[^1].FocusNeighborBottom;
    }

    private static float GetAvailableWidth(NRelicInventory inventory)
    {
        var width = inventory.Size.X;
        if (width <= 0f && inventory.GetParent() is Control parent)
        {
            width = parent.Size.X;
        }
        return width > 0f ? width : 1024f;
    }

    private static int GetPageSize(float width, float holderWidth, int separation)
    {
        return Math.Max(1, (int)Mathf.Floor((width + separation) / (holderWidth + separation)));
    }

    private static int GetMaxPage(int count, int pageSize) =>
        count == 0 ? 0 : (count - 1) / pageSize;

    private static float GetHolderWidth(NRelicInventoryHolder holder)
    {
        var width = Mathf.Max(holder.Size.X, holder.GetCombinedMinimumSize().X);
        return width > 1f ? width : FallbackHolderWidth;
    }

    private static float GetHolderHeight(IEnumerable<NRelicInventoryHolder> holders) =>
        holders.Select(static holder => Mathf.Max(holder.Size.Y, holder.GetCombinedMinimumSize().Y))
            .DefaultIfEmpty(FallbackHolderHeight)
            .Max();

    private static HBoxContainer? FindRow(NRelicInventory inventory, string rowName) =>
        inventory.GetNodeOrNull<HBoxContainer>(rowName);

    private sealed class PagingState
    {
        public int NormalPage;
        public int RecipePage;
    }
}

internal sealed partial class KillerMovePageArrow : NButton
{
    private readonly bool _left;
    private bool _highlighted;
    private bool _pressed;

    public KillerMovePageArrow(bool left)
    {
        _left = left;
        FocusMode = FocusModeEnum.All;
    }

    public override void _Ready()
    {
        ConnectSignals();
    }

    protected override void OnFocus()
    {
        base.OnFocus();
        _highlighted = true;
        QueueRedraw();
    }

    protected override void OnUnfocus()
    {
        base.OnUnfocus();
        _highlighted = false;
        QueueRedraw();
    }

    protected override void OnPress()
    {
        base.OnPress();
        _pressed = true;
        QueueRedraw();
    }

    protected override void OnRelease()
    {
        base.OnRelease();
        _pressed = false;
        QueueRedraw();
    }

    public override void _Draw()
    {
        var center = Size / 2f;
        var size = _pressed ? 10f : _highlighted ? 12f : 10f;
        var color = !IsEnabled
            ? new Color("665f55")
            : _highlighted
                ? new Color("f2d58b")
                : new Color("d8c6a2");
        var points = _left
            ? new Vector2[]
            {
                center + new Vector2(size, -size),
                center + new Vector2(-size, 0f),
                center + new Vector2(size, size)
            }
            : new Vector2[]
            {
                center + new Vector2(-size, -size),
                center + new Vector2(size, 0f),
                center + new Vector2(-size, size)
            };
        DrawColoredPolygon(points, color);
    }
}
