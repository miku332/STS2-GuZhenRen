using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Patching.Models;

namespace GuZhenRen.Patches;

public sealed class AiQingGuEscapeRewardPatch : IPatchMethod
{
    private static CombatRoom? _roomSkippingRewards;

    public static string PatchId => "ai_qing_gu_escape_reward";

    public static string Description =>
        "爱情蛊逃离战斗时跳过战斗奖励";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(typeof(NCombatUi), "OnCombatWon")
    ];

    public static void SkipRewardsFor(CombatRoom room)
    {
        _roomSkippingRewards = room;
    }

    public static bool Prefix(NCombatUi __instance, CombatRoom room)
    {
        if (!ReferenceEquals(room, _roomSkippingRewards))
        {
            return true;
        }

        _roomSkippingRewards = null;
        TaskHelper.RunSafely(__instance.ProceedWithoutRewards());
        return false;
    }
}
