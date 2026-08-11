using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using STS2RitsuLib.Patching.Models;

namespace GuZhenRen.Patches;

public sealed class CeremonialBeastStunPatch : IPatchMethod
{
    public static string PatchId => "ceremonial_beast_transition_stun";

    public static string Description =>
        "Allow the Ceremonial Beast phase-transition stun to replace a card stun";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(
            typeof(MonsterModel),
            nameof(MonsterModel.SetMoveImmediate),
            [typeof(MoveState), typeof(bool)])
    ];

    public static void Prefix(
        MonsterModel __instance,
        MoveState state,
        ref bool forceTransition)
    {
        if (__instance is CeremonialBeast beast
            && beast.IsInSecondPhase
            && state.Id == MonsterModel.stunnedMoveId
            && state.FollowUpStateId == beast.BeastCryState.StateId)
        {
            forceTransition = true;
        }
    }
}
