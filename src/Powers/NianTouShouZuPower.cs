using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class NianTouShouZuPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.None;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/NianTouShouZuPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/NianTouShouZuPower_p.png");

    public override async Task AfterApplied(
        Creature? applier,
        CardModel? cardSource)
    {
        await BlockExistingNian();
    }

    public void FlashBlocked()
    {
        Flash();
    }

    private async Task BlockExistingNian()
    {
        var nian = Owner.GetPower<NianPower>();
        if (nian is null)
        {
            return;
        }

        FlashBlocked();
        await PowerCmd.Remove(nian);
    }
}
