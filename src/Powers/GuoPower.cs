using MegaCrit.Sts2.Core.Combat;
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
public sealed class GuoPower : ModPowerTemplate
{
    private sealed class FruitState
    {
        public bool IsDamage { get; set; }
        public decimal Damage { get; set; }
        public PowerModel? StoredPower { get; set; }
        public int StoredPowerAmount { get; set; }
    }

    public static bool IsApplying { get; private set; }

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerInstanceType InstanceType => PowerInstanceType.Instanced;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/GuoPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/GuoPower_p.png");

    protected override object InitInternalData() => new FruitState();

    private static bool IsPlayerCardPower(PowerModel power) =>
        power is DampenPower or HexPower or RingingPower or TangledPower;

    public override LocString Description
    {
        get
        {
            var state = IsMutable ? GetInternalData<FruitState>() : null;
            var isDamage = state?.IsDamage ?? true;
            var description = new LocString(
                "powers",
                isDamage
                    ? "GU_ZHEN_REN_POWER_GUO_POWER.damage_description"
                    : "GU_ZHEN_REN_POWER_GUO_POWER.power_description");
            description.Add("Amount", Amount);
            description.Add("StoredDamage", state?.Damage ?? 0m);
            description.Add(
                "StoredPower",
                state?.StoredPower?.Title.GetFormattedText() ?? "?");
            description.Add("StoredPowerAmount", state?.StoredPowerAmount ?? 0);
            return description;
        }
    }

    internal static async Task<GuoPower?> CreateDamageFruit(
        PlayerChoiceContext choiceContext,
        Creature target,
        decimal damage,
        Creature? applier,
        CardModel? cardSource)
    {
        var fruit = await PowerCmd.Apply<GuoPower>(
            choiceContext,
            target,
            2,
            applier,
            cardSource);
        fruit?.StoreDamage(damage);
        return fruit;
    }

    internal void StoreDamage(decimal damage)
    {
        var state = GetInternalData<FruitState>();
        Entry.Logger.Info($"[Guo] Stored damage fruit={damage}.");
        state.IsDamage = true;
        state.Damage = damage;
        state.StoredPower = null;
        state.StoredPowerAmount = 0;
        InvokeDisplayAmountChanged();
    }

    internal void StorePower(PowerModel power, int amount)
    {
        var state = GetInternalData<FruitState>();
        state.IsDamage = false;
        state.Damage = 0m;
        state.StoredPower = power.IsCanonical
            ? power
            : (PowerModel)power.ClonePreservingMutability();
        state.StoredPowerAmount = amount;
        InvokeDisplayAmountChanged();
    }

    internal GuoPower CreateTransferCopy()
    {
        var copy = (GuoPower)ModelDb.Power<GuoPower>().ToMutable();
        var state = GetInternalData<FruitState>();
        if (state.IsDamage)
        {
            copy.StoreDamage(state.Damage);
        }
        else if (state.StoredPower is not null)
        {
            copy.StorePower(state.StoredPower, state.StoredPowerAmount);
        }

        return copy;
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (!Owner.IsAlive
            || !participants.Contains(Owner)
            || (Owner.IsPlayer && side != CombatSide.Player)
            || (Owner.IsEnemy && side != CombatSide.Enemy))
        {
            return;
        }

        if (Amount > 1)
        {
            await PowerCmd.Decrement(this);
            return;
        }

        var state = GetInternalData<FruitState>();
        Flash();
        IsApplying = true;
        try
        {
            if (state.IsDamage)
            {
                await CreatureCmd.Damage(
                    choiceContext,
                    Owner,
                    state.Damage,
                    ValueProp.Unpowered,
                    Owner,
                    null,
                    null);
            }
            else if (state.StoredPower is not null)
            {
                if (Owner.IsEnemy && IsPlayerCardPower(state.StoredPower))
                {
                    Entry.Logger.Info(
                        $"[Guo] Skipped player-only power {state.StoredPower.Id.Entry} on an enemy.");
                }
                else
                {
                    var replay = state.StoredPower.IsCanonical
                        ? state.StoredPower.ToMutable()
                        : (PowerModel)state.StoredPower.ClonePreservingMutability();
                    try
                    {
                        await PowerCmd.Apply(
                            choiceContext,
                            replay,
                            Owner,
                            state.StoredPowerAmount,
                            Applier,
                            null);
                    }
                    catch (Exception ex)
                    {
                        Entry.Logger.Error(
                            $"[Guo] Failed to apply stored power {state.StoredPower.Id.Entry}; "
                            + $"the incompatible fruit was discarded: {ex}");
                        if (Owner.GetPowerInstances(replay.Id).Contains(replay))
                        {
                            try
                            {
                                await PowerCmd.Remove(replay);
                            }
                            catch (Exception cleanupEx)
                            {
                                Entry.Logger.Error(
                                    $"[Guo] Cleanup for incompatible power {replay.Id.Entry} failed "
                                    + $"after it was detached: {cleanupEx}");
                            }
                        }
                    }
                }
            }
        }
        finally
        {
            IsApplying = false;
        }

        await PowerCmd.Remove(this);
    }
}
