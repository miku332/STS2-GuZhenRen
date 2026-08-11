using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class DouZhuanPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.None;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/DouZhuanPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/DouZhuanPower_p.png");

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

        var missingHp = Math.Max(0m, Owner.MaxHp - Owner.CurrentHp);
        if (Owner.CurrentHp >= missingHp)
        {
            return;
        }

        var healing = missingHp - Owner.CurrentHp;
        if (healing <= 0)
        {
            return;
        }

        Flash();
        await CreatureCmd.Heal(Owner, healing);
    }
}
