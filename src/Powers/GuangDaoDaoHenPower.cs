using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class GuangDaoDaoHenPower : AbstractDaoHenPower
{
    public const int DamagePercentPerStack = 25;

    public override LocString Description
    {
        get
        {
            var description = base.Description;
            description.Add("DamagePercent", Amount * DamagePercentPerStack);
            return description;
        }
    }

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (dealer != Owner
            || cardSource is null
            || !GuZhenRenTagRules.HasEffectiveTag(
                cardSource,
                GuZhenRenTags.GuangDao)
            || !props.IsPoweredAttack()
            || Owner.GetPower<ShanYaoPower>() is not null)
        {
            return 1m;
        }

        return 1m + Amount * DamagePercentPerStack / 100m;
    }
}
