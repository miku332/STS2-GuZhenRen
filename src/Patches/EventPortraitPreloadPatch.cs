using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Patching.Models;
using STS2RitsuLib.Scaffolding.Content.Patches;

namespace GuZhenRen.Patches;

public sealed class EventPortraitPreloadPatch : IPatchMethod
{
    public static string PatchId => "event-portrait-preload";

    public static string Description =>
        "Removes invalid fallback portrait paths from Gu Zhen Ren event preloading.";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(typeof(EventModel), "GetAssetPaths")
    ];

    public static void Postfix(
        EventModel __instance,
        ref IEnumerable<string> __result)
    {
        if (__instance is not IModEventAssetOverrides assetOverrides
            || !__instance.Id.Entry.StartsWith(
                "GU_ZHEN_REN_EVENT_",
                StringComparison.Ordinal)
            || string.IsNullOrWhiteSpace(assetOverrides.CustomInitialPortraitPath))
        {
            return;
        }

        var customPortraitPath = assetOverrides.CustomInitialPortraitPath;
        var fallbackPortraitPath =
            $"res://images/events/{__instance.Id.Entry.ToLowerInvariant()}.png";

        __result = __result
            .Where(path => !string.Equals(
                path,
                fallbackPortraitPath,
                StringComparison.Ordinal))
            .Append(customPortraitPath)
            .Distinct(StringComparer.Ordinal);
    }
}
