using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class HuoMaoSanZhangPower : ModPowerTemplate
{
    private const decimal MinimumAppliedBurn = 3;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.None;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/HuoMaoSanZhangPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/HuoMaoSanZhangPower_p.png");

    public static decimal GetAdjustedBurnAmount(
        Creature? applier,
        decimal amount)
    {
        if (amount <= 0 || amount >= MinimumAppliedBurn)
        {
            return amount;
        }

        var power = applier?.GetPower<HuoMaoSanZhangPower>();
        if (power is null)
        {
            return amount;
        }

        power.Flash();
        return MinimumAppliedBurn;
    }
}
