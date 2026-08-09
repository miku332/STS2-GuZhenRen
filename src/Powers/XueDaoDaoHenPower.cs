using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class XueDaoDaoHenPower : AbstractDaoHenPower
{
    public override async Task AfterDamageGiven(
        PlayerChoiceContext choiceContext,
        Creature? dealer,
        DamageResult result,
        ValueProp props,
        Creature target,
        CardModel? cardSource)
    {
        if (dealer != Owner
            || target == Owner
            || Amount <= 0
            || !props.IsPoweredAttack()
            || result.UnblockedDamage <= 0)
        {
            return;
        }

        var healing = (int)Math.Ceiling(
            result.UnblockedDamage * Amount * 0.01m);
        if (healing <= 0)
        {
            return;
        }

        Flash();
        await CreatureCmd.Heal(Owner, healing);
    }
}
