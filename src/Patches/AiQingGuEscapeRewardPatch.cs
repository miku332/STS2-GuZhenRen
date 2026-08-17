using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Screens;
using MegaCrit.Sts2.Core.Rewards;
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

        await MegaCrit.Sts2.Core.Commands.Cmd.Wait(1f);
        var runState = room.CombatState.RunState;
        if (LocalContext.GetMe(runState) is not { } player)
        {
            Entry.Logger.Warn(
                "Love Gu escape could not resolve the local player.");
            await combatUi.ProceedWithoutRewards();
            return;
        }

        var rewardsSet = new RewardsSet(player).EmptyForRoom(room);

        await RunManager.Instance.RewardsSetSynchronizer
            .BeginRewardsSet(rewardsSet);
        NRewardsScreen.ShowScreen(rewardsSet, true, runState);
        Entry.Logger.Info(
            "Love Gu escape opened an empty boss proceed screen.");
    }
}
