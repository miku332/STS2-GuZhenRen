using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class LuDaoDaoHenPower : AbstractDaoHenPower
{
    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        var combatState = Owner.CombatState;
        if (side != CombatSide.Player
            || !participants.Contains(Owner)
            || Amount <= 0
            || combatState is null)
        {
            return;
        }

        Flash();
        foreach (var enemy in combatState.HittableEnemies.ToList())
        {
            await PowerCmd.Apply<LuDaoStrengthDownPower>(
                choiceContext,
                enemy,
                Amount,
                Owner,
                null);
        }
    }
}
