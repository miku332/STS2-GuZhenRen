using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class ZhuanYiPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/ZhuanYiPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/ZhuanYiPower_p.png");

    public static async Task TriggerConversion(
        Creature owner,
        Creature? applier,
        CardModel? cardSource)
    {
        var power = owner.GetPower<ZhuanYiPower>();
        if (power is null || power.Amount <= 0)
        {
            return;
        }

        power.Flash();
        await CreatureCmd.GainBlock(
            owner,
            power.Amount,
            ValueProp.Unpowered,
            null,
            fast: true);
    }
}
