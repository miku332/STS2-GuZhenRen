using GuZhenRen.Relics;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using STS2RitsuLib.Patching.Models;

namespace GuZhenRen.Patches;

public sealed class GuiBuJueMoveTargetPatch : IPatchMethod
{
    public static string PatchId => "gui-bu-jue-move-targets";

    public static string Description =>
        "Ghost moves cannot target players protected by Gui Bu Jue.";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(
            typeof(MoveState),
            nameof(MoveState.PerformMove),
            [typeof(IEnumerable<Creature>)])
    ];

    public static void Prefix(
        MoveState __instance,
        ref IEnumerable<Creature> targets)
    {
        if (!HasPlayerTargetingIntent(__instance))
        {
            return;
        }

        var targetList = targets.ToList();
        var actingMonster = targetList
            .Select(target => target.CombatState)
            .Where(state => state is not null)
            .SelectMany(state => state!.Enemies)
            .Select(enemy => enemy.Monster)
            .FirstOrDefault(GuiBuJueTargeting.IsActingGhost);

        if (actingMonster is null)
        {
            return;
        }

        targets = GuiBuJueTargeting.FilterProtectedPlayers(
            targetList,
            flashRelic: true);
    }

    private static bool HasPlayerTargetingIntent(MoveState move) =>
        move.Intents.Any(intent => intent.IntentType is
            IntentType.Attack
            or IntentType.Debuff
            or IntentType.DebuffStrong
            or IntentType.StatusCard
            or IntentType.CardDebuff
            or IntentType.DeathBlow
            or IntentType.Unknown);
}

public sealed class GuiBuJueAttackTargetPatch : IPatchMethod
{
    public static string PatchId => "gui-bu-jue-attack-targets";

    public static string Description =>
        "Monster attacks also respect Gui Bu Jue target protection.";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(
            typeof(AttackCommand),
            "GetPossibleTargets",
            Type.EmptyTypes)
    ];

    public static void Postfix(
        AttackCommand __instance,
        ref IReadOnlyList<Creature> __result)
    {
        if (!GuiBuJueTargeting.IsActingGhost(__instance.Attacker?.Monster))
        {
            return;
        }

        __result = GuiBuJueTargeting.FilterProtectedPlayers(
            __result,
            flashRelic: false);
    }
}

internal static class GuiBuJueTargeting
{
    internal static bool IsActingGhost(MonsterModel? monster) =>
        monster is
            HauntedShip
            or PhantasmalGardener
            or SoulFysh
            or SoulNexus
            or SpectralKnight
            or TheForgotten
            or TheLost
            or TorchHeadAmalgam
            or Vantom
        && monster.IsPerformingMove;

    internal static IReadOnlyList<Creature> FilterProtectedPlayers(
        IEnumerable<Creature> targets,
        bool flashRelic)
    {
        var allowedTargets = new List<Creature>();
        foreach (var target in targets)
        {
            var relic = target.Player?.GetRelic<GuiBuJue>();
            if (relic is null)
            {
                allowedTargets.Add(target);
                continue;
            }

            if (flashRelic)
            {
                relic.Flash();
            }
        }

        return allowedTargets;
    }
}
