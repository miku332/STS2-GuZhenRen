using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class TemporaryHpPower : ModPowerTemplate
{
    private sealed class AbsorptionState
    {
        public decimal PendingAmount;
    }

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/ZhiZhangPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/ZhiZhangPower_p.png");

    protected override object InitInternalData() => new AbsorptionState();

    public override decimal ModifyHpLostAfterOstyLate(
        Creature target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        MegaCrit.Sts2.Core.Models.CardModel? cardSource)
    {
        if (target != Owner || amount <= 0 || Amount <= 0)
        {
            return amount;
        }

        var absorbed = Math.Min(Amount, amount);
        GetInternalData<AbsorptionState>().PendingAmount += absorbed;
        return amount - absorbed;
    }

    public override async Task AfterModifyingHpLostAfterOsty()
    {
        var state = GetInternalData<AbsorptionState>();
        var absorbed = state.PendingAmount;
        state.PendingAmount = 0;
        if (absorbed <= 0)
        {
            return;
        }

        Flash();
        await PowerCmd.ModifyAmount(
            new ThrowingPlayerChoiceContext(),
            this,
            -absorbed,
            Owner,
            null);
    }
}
