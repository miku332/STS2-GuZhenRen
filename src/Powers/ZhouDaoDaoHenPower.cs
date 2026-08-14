using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class ZhouDaoDaoHenPower : AbstractDaoHenPower
{
    public override async Task BeforeSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        var combatState = Owner.CombatState;
        var player = Owner.Player;
        if (side != CombatSide.Player
            || player is null
            || player.PlayerCombatState is not { } playerCombatState
            || !participants.Contains(Owner)
            || Amount <= 0
            || combatState is null)
        {
            return;
        }

        var hitCount = playerCombatState.TurnNumber;
        for (var hit = 0; hit < hitCount; hit++)
        {
            var enemies = combatState.HittableEnemies
                .Where(static enemy => enemy.IsAlive)
                .ToList();
            var target = player.RunState.Rng.CombatTargets.NextItem(enemies);
            if (target is null)
            {
                break;
            }

            Flash();
            await CreatureCmd.Damage(
                choiceContext,
                target,
                Amount,
                ValueProp.Unpowered,
                Owner,
                null,
                null);
        }
    }
}
