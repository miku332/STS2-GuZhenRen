using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Combat.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class LuDaoStrengthDownPower
    : ModTemporaryAppliedPowerTemplate<LuDaoDaoHenPower, StrengthPower>
{
    protected override bool IsPositive => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/LuDaoDaoHenPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/LuDaoDaoHenPower_p.png");

    public override LocString Description => new(
        "powers",
        "GU_ZHEN_REN_POWER_LU_DAO_STRENGTH_DOWN_POWER.description");
}
