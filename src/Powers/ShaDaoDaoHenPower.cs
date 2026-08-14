using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class ShaDaoDaoHenPower : AbstractDaoHenPower
{
    private const int DamagePerStack = 3;

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        var combatState = Owner.CombatState;
        if (!participants.Contains(Owner)
            || Amount <= 0
            || combatState is null)
        {
            return;
        }

        Flash();
        var damage = Amount * DamagePerStack;
        foreach (var enemy in combatState.HittableEnemies.ToList())
        {
            await CreatureCmd.Damage(
                choiceContext,
                enemy,
                damage,
                ValueProp.Unpowered,
                Owner,
                null,
                null);
        }
    }
}
