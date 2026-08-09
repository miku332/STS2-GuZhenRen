using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class LiDaoDaoHenPower : AbstractDaoHenPower
{
    private sealed class Data
    {
        public int StrengthGranted;
    }

    protected override object InitInternalData() => new Data();

    public override int GetDerivedPowerAmount(PowerModel power) =>
        power is StrengthPower
            ? GetInternalData<Data>().StrengthGranted
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
        GetInternalData<Data>().StrengthGranted += delta;
        await PowerCmd.Apply<StrengthPower>(
            choiceContext,
            Owner,
            delta,
            Owner,
            null);
    }

    public override async Task AfterRemoved(Creature oldOwner)
    {
        await RemoveGrantedStrength(
            new ThrowingPlayerChoiceContext(),
            oldOwner);
    }

    protected override Task BeforeResetToBianHua(
        PlayerChoiceContext choiceContext,
        Creature owner) => RemoveGrantedStrength(choiceContext, owner);

    private async Task RemoveGrantedStrength(
        PlayerChoiceContext choiceContext,
        Creature owner)
    {
        var data = GetInternalData<Data>();
        if (data.StrengthGranted <= 0)
        {
            return;
        }

        var granted = data.StrengthGranted;
        data.StrengthGranted = 0;
        var strength = owner.GetPower<StrengthPower>();
        if (strength is not null)
        {
            await PowerCmd.ModifyAmount(
                choiceContext,
                strength,
                -granted,
                owner,
                null);
        }
    }
}
