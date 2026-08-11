using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class JinDaoDaoHenPower : AbstractDaoHenPower
{
    private sealed class Data
    {
        public int PlatingGranted;
    }

    protected override object InitInternalData() => new Data();

    public override int GetDerivedPowerAmount(PowerModel power) =>
        power is PlatingPower
            ? GetInternalData<Data>().PlatingGranted
            : 0;

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        if (power != this || amount <= 0)
        {
            return;
        }

        var delta = (int)amount;
        GetInternalData<Data>().PlatingGranted += delta;
        await PowerCmd.Apply<PlatingPower>(
            choiceContext,
            Owner,
            delta,
            Owner,
            null);
    }

    public override async Task AfterRemoved(Creature oldOwner)
    {
        await RemoveGrantedPlating(
            new ThrowingPlayerChoiceContext(),
            oldOwner);
    }

    protected override Task BeforeResetToBianHua(
        PlayerChoiceContext choiceContext,
        Creature owner) => RemoveGrantedPlating(choiceContext, owner);

    private async Task RemoveGrantedPlating(
        PlayerChoiceContext choiceContext,
        Creature owner)
    {
        var data = GetInternalData<Data>();
        if (data.PlatingGranted <= 0)
        {
            return;
        }

        var granted = data.PlatingGranted;
        data.PlatingGranted = 0;
        var plating = owner.GetPower<PlatingPower>();
        if (plating is not null)
        {
            await PowerCmd.ModifyAmount(
                choiceContext,
                plating,
                -granted,
                owner,
                null);
        }
    }
}
