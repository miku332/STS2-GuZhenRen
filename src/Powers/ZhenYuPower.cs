using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class ZhenYuPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.None;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/ZhenYuPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/ZhenYuPower_p.png");

    public override bool ShouldDraw(Player player, bool fromHandDraw)
    {
        return !Owner.IsAlive
            || player.Creature != GetAffectedPlayer()
            || fromHandDraw;
    }

    public override Task AfterPreventingDraw()
    {
        Flash();
        return Task.CompletedTask;
    }

    private MegaCrit.Sts2.Core.Entities.Creatures.Creature? GetAffectedPlayer() =>
        Applier?.Player is not null
            ? Applier
            : Owner.CombatState?.Players.FirstOrDefault()?.Creature;
}
