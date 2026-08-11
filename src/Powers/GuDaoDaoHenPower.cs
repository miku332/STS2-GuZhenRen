using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class GuDaoDaoHenPower : AbstractDaoHenPower
{
    private const int ThornsPerMark = 2;

    private sealed class Data
    {
        public int ThornsGranted;
    }

    protected override object InitInternalData() => new Data();

    public override int GetDerivedPowerAmount(PowerModel power) =>
        power is ThornsPower
            ? GetInternalData<Data>().ThornsGranted
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

        var thorns = (int)amount * ThornsPerMark;
        GetInternalData<Data>().ThornsGranted += thorns;
        await PowerCmd.Apply<ThornsPower>(
            choiceContext,
            Owner,
            thorns,
            Owner,
            null);
    }

    public override async Task AfterRemoved(Creature oldOwner)
    {
        await RemoveGrantedThorns(
            new ThrowingPlayerChoiceContext(),
            oldOwner);
    }

    protected override Task BeforeResetToBianHua(
        PlayerChoiceContext choiceContext,
        Creature owner) => RemoveGrantedThorns(choiceContext, owner);

    private async Task RemoveGrantedThorns(
        PlayerChoiceContext choiceContext,
        Creature owner)
    {
        var data = GetInternalData<Data>();
        if (data.ThornsGranted <= 0)
        {
            return;
        }

        var granted = data.ThornsGranted;
        data.ThornsGranted = 0;
        var thorns = owner.GetPower<ThornsPower>();
        if (thorns is not null)
        {
            await PowerCmd.ModifyAmount(
                choiceContext,
                thorns,
                -granted,
                owner,
                null);
        }
    }
}
