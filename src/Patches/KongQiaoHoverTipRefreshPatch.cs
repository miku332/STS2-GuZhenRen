using System.Collections;
using System.Reflection;
using GuZhenRen.Relics;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.Nodes.Relics;
using STS2RitsuLib.Patching.Models;

namespace GuZhenRen.Patches;

/// <summary>
/// Rebuilds an already-open aperture relic tooltip after its tribulation state changes.
/// The relic model raises its normal display update event, but the vanilla tooltip node
/// has already copied the old text and does not rebuild itself.
/// </summary>
public sealed class KongQiaoHoverTipRefreshPatch : IPatchMethod
{
    private static readonly FieldInfo? ActiveHoverTipsField =
        typeof(NHoverTipSet).GetField(
            "_activeHoverTips",
            BindingFlags.Static | BindingFlags.NonPublic);

    public static string PatchId => "kong-qiao-hover-tip-refresh";

    public static string Description =>
        "Refreshes an open aperture relic tooltip when tribulation state changes.";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(
            typeof(NRelicInventoryHolder),
            "OnDisplayAmountChanged",
            Type.EmptyTypes)
    ];

    public static void Postfix(NRelicInventoryHolder __instance)
    {
        if (__instance.Relic.Model is not AbstractKongQiaoRelic)
        {
            return;
        }

        if (!HasActiveHoverTip(__instance))
        {
            return;
        }

        NHoverTipSet.Remove(__instance);
        Godot.Callable.From(() => RecreateHoverTip(__instance)).CallDeferred();
    }

    private static void RecreateHoverTip(NRelicInventoryHolder holder)
    {
        if (!Godot.GodotObject.IsInstanceValid(holder)
            || holder.IsQueuedForDeletion()
            || !holder.IsInsideTree())
        {
            return;
        }

        NHoverTipSet.CreateAndShow(holder, holder.Relic.Model.HoverTips)
            ?.SetAlignmentForRelic(holder.Relic);
    }

    private static bool HasActiveHoverTip(NRelicInventoryHolder holder) =>
        ActiveHoverTipsField?.GetValue(null) is IDictionary activeHoverTips
        && activeHoverTips.Contains(holder);
}
