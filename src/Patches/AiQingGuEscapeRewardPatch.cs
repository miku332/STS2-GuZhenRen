using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Patching.Models;

namespace GuZhenRen.Patches;

public sealed class AiQingGuEscapeRewardPatch : IPatchMethod
{
    private static CombatRoom? _roomSkippingRewards;

    public static string PatchId => "ai_qing_gu_escape_reward";

    public static string Description =>
        "爱情蛊逃离战斗时跳过奖励并进入地图";

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

        // Let the native victory flow finish before registering the terminal
        // rewards screen. This keeps the Boss room transition available.
        await MegaCrit.Sts2.Core.Commands.Cmd.Wait(1f);

        foreach (var player in room.CombatState.Players)
        {
            var rewardsSet = new RewardsSet(player).EmptyForRoom(room);
            _ = TaskHelper.RunSafely(rewardsSet.Offer());
        }

        Entry.Logger.Info(
            "Love Gu escape opened synchronized empty Boss proceed screens.");
    }
}
