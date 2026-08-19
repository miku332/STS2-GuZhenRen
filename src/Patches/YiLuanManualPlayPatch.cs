using GuZhenRen.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Patching.Models;

namespace GuZhenRen.Patches;

public sealed class YiLuanManualPlayPatch : IPatchMethod
{
    public static string PatchId => "yi-luan-manual-play";

    public static string Description =>
        "Yi Luan interferes with killer moves when a manual play action executes.";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(
            typeof(PlayCardAction),
            "ExecuteAction",
            Type.EmptyTypes)
    ];

    public static bool Prefix(
        PlayCardAction __instance,
        ref Task __result)
    {
        if (!YiLuan.TryBlockManualPlay(__instance, out var blockTask))
        {
            return true;
        }

        __result = blockTask;
        return false;
    }
}
