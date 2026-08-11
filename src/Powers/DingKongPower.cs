using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Combat.HandSize;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class DingKongPower : ModPowerTemplate, IMaxHandSizeModifier
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.None;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/DingKongPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/DingKongPower_p.png");

    public int ModifyMaxHandSize(Player player, int currentMaxHandSize)
    {
        if (!Owner.IsAlive
            || (Applier?.Player is not null && player.Creature != Applier))
        {
            return currentMaxHandSize;
        }

        return Math.Min(currentMaxHandSize, 5);
    }
}
