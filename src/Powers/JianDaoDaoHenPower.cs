using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class JianDaoDaoHenPower : AbstractDaoHenPower
{
    private sealed class Data
    {
        public int JianFengGranted;
    }

    protected override object InitInternalData() => new Data();

    public override int GetDerivedPowerAmount(PowerModel power) =>
        power is JianFengPower
            ? GetInternalData<Data>().JianFengGranted
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
        GetInternalData<Data>().JianFengGranted += delta;
        await PowerCmd.Apply<JianFengPower>(
            choiceContext,
            Owner,
            delta,
            Owner,
            null);
    }

    public override async Task AfterRemoved(Creature oldOwner)
    {
        await RemoveGrantedJianFeng(
            new ThrowingPlayerChoiceContext(),
            oldOwner);
    }

    protected override Task BeforeResetToBianHua(
        PlayerChoiceContext choiceContext,
        Creature owner) => RemoveGrantedJianFeng(choiceContext, owner);

    private async Task RemoveGrantedJianFeng(
        PlayerChoiceContext choiceContext,
        Creature owner)
    {
        var data = GetInternalData<Data>();
        if (data.JianFengGranted <= 0)
        {
            return;
        }

        var granted = data.JianFengGranted;
        data.JianFengGranted = 0;
        var jianFeng = owner.GetPower<JianFengPower>();
        if (jianFeng is not null)
        {
            await PowerCmd.ModifyAmount(
                choiceContext,
                jianFeng,
                -granted,
                owner,
                null);
        }
    }
}
