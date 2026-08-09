using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using STS2RitsuLib.Interop.AutoRegistration;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class MuDaoDaoHenPower : AbstractDaoHenPower
{
    private sealed class Data
    {
        public bool IsApplyingBonusHealing;
    }

    protected override object InitInternalData() => new Data();

    public override async Task AfterCurrentHpChanged(
        Creature creature,
        decimal delta)
    {
        var data = GetInternalData<Data>();
        if (creature != Owner
            || delta <= 0
            || Amount <= 0
            || data.IsApplyingBonusHealing)
        {
            return;
        }

        var bonus = (int)Math.Floor(delta * Amount * 0.15m + 0.5m);
        if (bonus <= 0)
        {
            return;
        }

        Flash();
        data.IsApplyingBonusHealing = true;
        try
        {
            await CreatureCmd.Heal(Owner, bonus);
        }
        finally
        {
            data.IsApplyingBonusHealing = false;
        }
    }
}
