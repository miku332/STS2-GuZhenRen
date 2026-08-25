using System.Collections.Concurrent;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class TouDaoDaoHenPower : AbstractDaoHenPower
{
    private const int MaxGoldPerCombat = 30;
    private const string RemainingKey = "Remaining";

    private static readonly ConcurrentDictionary<ulong, int>
        TotalGoldStolenThisCombat = [];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(RemainingKey, MaxGoldPerCombat)
    ];

    public static void ResetCombatState() => TotalGoldStolenThisCombat.Clear();

    private int RemainingGold =>
        Math.Max(
            0,
            MaxGoldPerCombat
            - TotalGoldStolenThisCombat.GetValueOrDefault(Owner.Player!.NetId));

    public override LocString Description
    {
        get
        {
            var description = base.Description;
            description.Add(RemainingKey, RemainingGold);
            return description;
        }
    }

    public override async Task AfterDamageGiven(
        PlayerChoiceContext choiceContext,
        Creature? dealer,
        DamageResult result,
        ValueProp props,
        Creature target,
        CardModel? cardSource)
    {
        if (dealer != Owner
            || !target.IsEnemy
            || Amount <= 0
            || !props.IsPoweredAttack()
            || result.UnblockedDamage <= 0
            || RemainingGold <= 0
            || Owner.Player is null)
        {
            return;
        }

        var goldToSteal = Math.Min(Amount, RemainingGold);
        TotalGoldStolenThisCombat.AddOrUpdate(
            Owner.Player!.NetId,
            goldToSteal,
            (_, current) => current + goldToSteal);

        Flash();
        await PlayerCmd.GainGold(goldToSteal, Owner.Player);
    }
}
