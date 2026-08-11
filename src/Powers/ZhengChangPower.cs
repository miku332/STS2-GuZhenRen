using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class ZhengChangPower : ModPowerTemplate
{
    private const int ResetTurns = 3;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/ZhengChangPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/ZhengChangPower_p.png");

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Enemy
            || !Owner.IsAlive
            || !participants.Contains(Owner))
        {
            return;
        }

        if (Amount > 1)
        {
            Flash();
            await PowerCmd.Decrement(this);
            return;
        }

        var target = Applier?.Player is not null && Applier.IsAlive
            ? Applier
            : Owner.CombatState?.Players.FirstOrDefault(player =>
                player.Creature.IsAlive)?.Creature;
        if (target is null)
        {
            return;
        }

        Flash();
        var removablePowers = target.Powers
            .Where(power => power is not PlayerTribulationPower
                && power is not YongShengPower)
            .ToList();
        foreach (var power in removablePowers)
        {
            await PowerCmd.Remove(power);
        }

        if (Owner.IsAlive)
        {
            await PowerCmd.ModifyAmount(
                choiceContext,
                this,
                ResetTurns - Amount,
                Owner,
                null);
        }
    }
}
