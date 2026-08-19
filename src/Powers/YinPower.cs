using GuZhenRen.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class YinPower : ModPowerTemplate
{
    private sealed class PendingPower
    {
        public required PowerModel Model { get; init; }
        public required int Amount { get; init; }
    }

    private sealed class YinState
    {
        public decimal? PendingDamage { get; set; }
        public Queue<PendingPower> PendingPowers { get; } = new();
    }

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/YinPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/YinPower_p.png");

    protected override object InitInternalData() => new YinState();

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (target != Owner
            || amount <= 0
            || GuoPower.IsApplying
            || !props.HasFlag(ValueProp.Move))
        {
            return 1m;
        }

        GetInternalData<YinState>().PendingDamage = amount;
        return 0m;
    }

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner || GuoPower.IsApplying)
        {
            return;
        }

        var state = GetInternalData<YinState>();
        if (!state.PendingDamage.HasValue)
        {
            return;
        }

        var damage = state.PendingDamage.Value;
        state.PendingDamage = null;
        Entry.Logger.Info(
            $"[Yin] Resolved attack captured={damage}, blocked={result.BlockedDamage}, "
            + $"unblocked={result.UnblockedDamage}, fullyBlocked={result.WasFullyBlocked}.");
        if (damage <= 0 || result.WasFullyBlocked)
        {
            return;
        }

        Flash();
        await GuoPower.CreateDamageFruit(
            choiceContext,
            Owner,
            damage,
            Owner,
            cardSource);
    }

    public override bool TryModifyPowerAmountReceived(
        PowerModel canonicalPower,
        Creature target,
        decimal amount,
        Creature? applier,
        out decimal modifiedAmount)
    {
        modifiedAmount = amount;
        if (target != Owner
            || amount == 0
            || GuoPower.IsApplying
            || canonicalPower is GuoPower
            || canonicalPower.GetTypeForAmount(amount) != PowerType.Debuff)
        {
            return false;
        }

        GetInternalData<YinState>().PendingPowers.Enqueue(new PendingPower
        {
            Model = canonicalPower,
            Amount = (int)amount
        });
        Flash();
        modifiedAmount = 0m;
        return true;
    }

    public override async Task AfterModifyingPowerAmountReceived(PowerModel power)
    {
        var state = GetInternalData<YinState>();
        if (state.PendingPowers.Count == 0)
        {
            return;
        }

        var pending = state.PendingPowers.Dequeue();
        var fruit = await PowerCmd.Apply<GuoPower>(
            new ThrowingPlayerChoiceContext(),
            Owner,
            2,
            Owner,
            null);
        fruit?.StorePower(pending.Model, pending.Amount);
    }
}
