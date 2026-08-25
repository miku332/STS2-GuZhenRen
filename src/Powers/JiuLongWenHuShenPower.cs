using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class JiuLongWenHuShenPower : ModPowerTemplate
{
    private const int DamageReductionStacks = 9;
    private const int ExtraStacksPerAdditionalPlayer = 3;

    internal static int GetMaxStacks(Creature owner)
    {
        var playerCount = owner.CombatState?.Players.Count ?? 1;
        return DamageReductionStacks
            + Math.Max(0, playerCount - 1) * ExtraStacksPerAdditionalPlayer;
    }

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override LocString Description
    {
        get
        {
            var description = new LocString(
                "powers",
                "GU_ZHEN_REN_POWER_JIU_LONG_WEN_HU_SHEN_POWER.description");
            description.Add(
                "Reduction",
                Math.Clamp(Amount, 0, DamageReductionStacks) * 10);
            return description;
        }
    }

    public override PowerAssetProfile AssetProfile => new(
        IconPath:
            "res://GuZhenRen/images/powers/JiuLongWenHuShenPower.png",
        BigIconPath:
            "res://GuZhenRen/images/powers/JiuLongWenHuShenPower_p.png");

    public override Task AfterApplied(
        Creature? applier,
        CardModel? cardSource)
    {
        var maxStacks = GetMaxStacks(Owner);
        if (Amount > maxStacks)
        {
            SetAmount(maxStacks);
        }

        return Task.CompletedTask;
    }

    public override Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource)
    {
        var maxStacks = GetMaxStacks(Owner);
        if (power == this && Amount > maxStacks)
        {
            SetAmount(maxStacks);
        }

        return Task.CompletedTask;
    }

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (target != Owner
            || Amount <= 0
            || props.HasFlag(ValueProp.Unblockable))
        {
            return 1m;
        }

        return 1m
            - Math.Clamp(Amount, 0, DamageReductionStacks) / 10m;
    }

    public override decimal ModifyHpLostBeforeOstyLate(
        Creature target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner
            || Amount <= 0
            || amount <= 0
            || props.HasFlag(ValueProp.Unblockable))
        {
            return amount;
        }

        return Math.Max(amount, 1m);
    }

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner
            || Amount <= 0
            || result.UnblockedDamage <= 0
            || props.HasFlag(ValueProp.Unblockable))
        {
            return;
        }

        Flash();
        await PowerCmd.Decrement(this);
    }
}
