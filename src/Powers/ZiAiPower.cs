using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class ZiAiPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.None;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/ZiAiPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/ZiAiPower_p.png");

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

        var debuffs = Owner.Powers
            .Where(power => power.TypeForCurrentAmount == PowerType.Debuff)
            .ToList();
        if (debuffs.Count == 0)
        {
            return;
        }

        Flash();
        foreach (var debuff in debuffs)
        {
            await PowerCmd.Remove(debuff);
        }
    }
}
