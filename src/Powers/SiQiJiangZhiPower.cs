using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class SiQiJiangZhiPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/SiQiJiangZhiPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/SiQiJiangZhiPower_p.png");

    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
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
            : combatState.Players.FirstOrDefault(player =>
                player.Creature.IsAlive)?.Creature;
        if (target is null)
        {
            return;
        }

        Flash();
        await CreatureCmd.Kill(target);
        if (Owner.IsAlive)
        {
            await PowerCmd.Remove(this);
        }
    }
}
