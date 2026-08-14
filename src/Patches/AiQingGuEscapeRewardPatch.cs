using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
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
        TaskHelper.RunSafely(ProceedAfterEscape(__instance, room));
        return false;
    }

    private static async Task ProceedAfterEscape(
        NCombatUi combatUi,
        CombatRoom room)
    {
        if (room.RoomType != RoomType.Boss)
        {
            await combatUi.ProceedWithoutRewards();
            return;
        }

        var runState = room.CombatState.RunState;
        if (runState.Map.SecondBossMapPoint is not null
            && runState.CurrentMapCoord == runState.Map.BossMapPoint.coord)
        {
            await combatUi.ProceedWithoutRewards();
            return;
        }

        await MegaCrit.Sts2.Core.Commands.Cmd.Wait(1f);
        RunManager.Instance.ActChangeSynchronizer.SetLocalPlayerReady();
    }
}
