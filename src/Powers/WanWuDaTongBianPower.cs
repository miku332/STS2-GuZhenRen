using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class WanWuDaTongBianPower : ModPowerTemplate
{
    private sealed class ConversionState
    {
        public bool IsConverting;
    }

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.None;

    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/WanWuDaTongBianPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/WanWuDaTongBianPower_p.png");

    protected override object InitInternalData() => new ConversionState();

    public override async Task AfterApplied(
        Creature? applier,
        CardModel? cardSource)
    {
        await ConvertExistingPowers(
            new ThrowingPlayerChoiceContext(),
            applier ?? Owner,
            cardSource);
    }

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        var state = GetInternalData<ConversionState>();
        if (state.IsConverting
            || amount <= 0
            || power.Owner != Owner
            || IsProtectedPower(power))
        {
            return;
        }

        state.IsConverting = true;
        try
        {
            var convertedAmount = GetConvertibleAmount(power);
            if (convertedAmount <= 0)
            {
                return;
            }

            Flash();
            var activeDaoHenType = GetActiveDaoHenType();
            await KeepOnlyDerivedAmount(choiceContext, power, applier, cardSource);
            await ApplyConvertedDaoHen(
                choiceContext,
                activeDaoHenType,
                convertedAmount,
                applier ?? Owner,
                cardSource);
        }
        finally
        {
            state.IsConverting = false;
        }
    }

    private async Task ConvertExistingPowers(
        PlayerChoiceContext choiceContext,
        Creature applier,
        CardModel? cardSource)
    {
        var state = GetInternalData<ConversionState>();
        if (state.IsConverting)
        {
            return;
        }

        state.IsConverting = true;
        try
        {
            var activeDaoHenType = GetActiveDaoHenType();
            var powersToConvert = Owner.Powers
                .Where(power => power != this && !IsProtectedPower(power))
                .ToList();
            var totalConverted = 0;

            foreach (var power in powersToConvert)
            {
                var convertedAmount = GetConvertibleAmount(power);
                if (convertedAmount <= 0)
                {
                    continue;
                }

                totalConverted += convertedAmount;
                await KeepOnlyDerivedAmount(
                    choiceContext,
                    power,
                    applier,
                    cardSource);
            }

            if (totalConverted <= 0)
            {
                return;
            }

            Flash();
            await ApplyConvertedDaoHen(
                choiceContext,
                activeDaoHenType,
                totalConverted,
                applier,
                cardSource);
        }
        finally
        {
            state.IsConverting = false;
        }
    }

    private int GetConvertibleAmount(PowerModel power)
    {
        var currentAmount = Math.Max(0, power.Amount);
        var derivedAmount = Math.Clamp(
            GetDerivedAmount(power),
            0,
            currentAmount);

        if (currentAmount == 0)
        {
            return derivedAmount == 0 ? 1 : 0;
        }

        return currentAmount - derivedAmount;
    }

    private int GetDerivedAmount(PowerModel power) =>
        Owner.Powers
            .OfType<AbstractDaoHenPower>()
            .Sum(daoHen => daoHen.GetDerivedPowerAmount(power));

    private async Task KeepOnlyDerivedAmount(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        Creature? applier,
        CardModel? cardSource)
    {
        var currentAmount = Math.Max(0, power.Amount);
        var derivedAmount = Math.Clamp(
            GetDerivedAmount(power),
            0,
            currentAmount);

        if (derivedAmount <= 0)
        {
            await PowerCmd.Remove(power);
            return;
        }

        var amountToRemove = currentAmount - derivedAmount;
        if (amountToRemove > 0)
        {
            await PowerCmd.ModifyAmount(
                choiceContext,
                power,
                -amountToRemove,
                applier,
                cardSource);
        }
    }

    private Type? GetActiveDaoHenType() =>
        Owner.Powers
            .FirstOrDefault(power => power is AbstractDaoHenPower && power.Amount > 0)
            ?.GetType();

    private async Task ApplyConvertedDaoHen(
        PlayerChoiceContext choiceContext,
        Type? activeDaoHenType,
        int amount,
        Creature applier,
        CardModel? cardSource)
    {
        if (activeDaoHenType == typeof(LiDaoDaoHenPower))
        {
            await PowerCmd.Apply<LiDaoDaoHenPower>(choiceContext, Owner, amount, applier, cardSource);
        }
        else if (activeDaoHenType == typeof(YanDaoDaoHenPower))
        {
            await PowerCmd.Apply<YanDaoDaoHenPower>(choiceContext, Owner, amount, applier, cardSource);
        }
        else if (activeDaoHenType == typeof(JianDaoDaoHenPower))
        {
            await PowerCmd.Apply<JianDaoDaoHenPower>(choiceContext, Owner, amount, applier, cardSource);
        }
        else if (activeDaoHenType == typeof(XueDaoDaoHenPower))
        {
            await PowerCmd.Apply<XueDaoDaoHenPower>(choiceContext, Owner, amount, applier, cardSource);
        }
        else if (activeDaoHenType == typeof(GuangDaoDaoHenPower))
        {
            await PowerCmd.Apply<GuangDaoDaoHenPower>(choiceContext, Owner, amount, applier, cardSource);
        }
        else if (activeDaoHenType == typeof(FengDaoDaoHenPower))
        {
            await PowerCmd.Apply<FengDaoDaoHenPower>(choiceContext, Owner, amount, applier, cardSource);
        }
        else if (activeDaoHenType == typeof(TuDaoDaoHenPower))
        {
            await PowerCmd.Apply<TuDaoDaoHenPower>(choiceContext, Owner, amount, applier, cardSource);
        }
        else if (activeDaoHenType == typeof(MuDaoDaoHenPower))
        {
            await PowerCmd.Apply<MuDaoDaoHenPower>(choiceContext, Owner, amount, applier, cardSource);
        }
        else
        {
            await PowerCmd.Apply<BianHuaDaoDaoHenPower>(choiceContext, Owner, amount, applier, cardSource);
        }
    }

    private static bool IsProtectedPower(PowerModel power) =>
        power is WanWuDaTongBianPower
        or BianHuaDaoDaoHenPower
        or AbstractDaoHenPower
        or ShanYaoHistoryPower
        || power.GetType().Name is "PlayerTribulationPower" or "YongShengPower";
}
