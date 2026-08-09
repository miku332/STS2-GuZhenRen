using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class FengDaoDaoHenPower : AbstractDaoHenPower
{
    public override async Task AfterCardDrawn(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool fromHandDraw)
    {
        var combatState = Owner.CombatState;
        if (card.Owner.Creature != Owner
            || Amount <= 0
            || combatState is null)
        {
            return;
        }

        Flash();
        foreach (var enemy in combatState.HittableEnemies.ToList())
        {
            await CreatureCmd.Damage(
                choiceContext,
                enemy,
                Amount,
                ValueProp.Unpowered,
                Owner,
                null);
        }
    }
}
