using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class GuangDaoDaoHenPower : AbstractDaoHenPower
{
    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (dealer != Owner
            || cardSource is null
            || !cardSource.Tags.Contains(GuZhenRenTags.GuangDao)
            || !props.IsPoweredAttack()
            || Owner.GetPower<ShanYaoPower>() is not null)
        {
            return 1m;
        }

        return 1m + Amount * 0.25m;
    }
}
