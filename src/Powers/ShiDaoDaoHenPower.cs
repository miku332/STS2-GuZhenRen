using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class ShiDaoDaoHenPower : AbstractDaoHenPower
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
            || target.Monster is null
            || Amount <= 0
            || !props.IsPoweredAttack()
            || !result.WasTargetKilled
            || target.GetPower<MinionPower>() is not null)
        {
            return;
        }

        Flash();
        await CreatureCmd.Heal(Owner, Amount);
    }
}
