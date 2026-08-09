using GuZhenRen.Relics;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace GuZhenRen.Patches;

internal static class NiLiuHeReflectionState
{
    private static readonly Dictionary<Creature, Creature> _reflectedDealers = [];

    public static bool TryRedirectAttack(
        Creature target,
        ValueProp props,
        Creature? dealer,
        out Creature redirectedTarget)
    {
        redirectedTarget = target;

        if (!target.IsPlayer
            || dealer is null
            || !dealer.IsMonster
            || !props.IsPoweredAttack())
        {
            return false;
        }

        var relic = target.Player?.GetRelic<NiLiuHe>();
        if (relic is null || !relic.TryConsumeWater())
        {
            _reflectedDealers.Remove(target);
            return false;
        }

        _reflectedDealers[target] = dealer;
        redirectedTarget = dealer;
        return true;
    }

    public static bool TryRedirectPower(
        PowerModel power,
        Creature target,
        decimal amount,
        Creature? applier,
        out Creature redirectedTarget)
    {
        redirectedTarget = target;
        if (power.GetTypeForAmount(amount) != PowerType.Debuff
            || !WasLastAttackReflected(target, applier))
        {
            return false;
        }

        redirectedTarget = applier!;
        return true;
    }

    public static bool TryRedirectDebuffLookup(
        PowerModel power,
        Creature target,
        Creature? applier,
        out Creature redirectedTarget)
    {
        redirectedTarget = target;
        if (power.Type != PowerType.Debuff
            || !WasLastAttackReflected(target, applier))
        {
            return false;
        }

        redirectedTarget = applier!;
        return true;
    }

    public static bool WasLastAttackReflected(
        Creature target,
        Creature? applier) =>
        target.IsPlayer
        && applier is not null
        && _reflectedDealers.TryGetValue(target, out var dealer)
        && ReferenceEquals(dealer, applier);

    public static void Clear() => _reflectedDealers.Clear();
}
