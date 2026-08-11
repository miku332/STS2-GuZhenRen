using GuZhenRen.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using STS2RitsuLib.Patching.Models;

namespace GuZhenRen.Patches;

public sealed class GuiGuaYiIntentPatch : IPatchMethod
{
    public static string PatchId => "hao_jie_gui_gua_yi_intent";

    public static string Description =>
        "Gui Gua Yi follows immediate monster intent changes";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(
            typeof(MonsterModel),
            nameof(MonsterModel.SetMoveImmediate),
            [typeof(MoveState), typeof(bool)])
    ];

    public static void Postfix(MonsterModel __instance)
    {
        var power = __instance.Creature?.GetPower<GuiGuaYiPower>();
        if (power is null)
        {
            return;
        }

        TaskHelper.RunSafely(power.SyncToCurrentIntent(
            new ThrowingPlayerChoiceContext()));
    }
}
