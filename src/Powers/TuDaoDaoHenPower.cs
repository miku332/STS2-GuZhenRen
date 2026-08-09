using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class TuDaoDaoHenPower : AbstractDaoHenPower
{
    private sealed class Data
    {
        public bool IsGrantingExtraBlock;
    }

    protected override object InitInternalData() => new Data();

    public override async Task AfterBlockGained(
        Creature creature,
        decimal amount,
        ValueProp props,
        CardModel? cardSource)
    {
        var data = GetInternalData<Data>();
        if (creature != Owner
            || amount <= 0
            || Amount <= 0
            || data.IsGrantingExtraBlock)
        {
            return;
        }

        Flash();
        data.IsGrantingExtraBlock = true;
        try
        {
            await CreatureCmd.GainBlock(
                Owner,
                Amount,
                ValueProp.Unpowered,
                null,
                fast: true);
        }
        finally
        {
            data.IsGrantingExtraBlock = false;
        }
    }
}
