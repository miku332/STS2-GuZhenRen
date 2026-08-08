using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Combat.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.Cards;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class LiLiangGuStrengthDownPower
    : ModTemporaryAppliedPowerTemplate<LiLiangGu, StrengthPower>
{
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/LiLiangGuStrengthDownPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/LiLiangGuStrengthDownPower_p.png");

    public override LocString Description => new(
        "powers",
        "GU_ZHEN_REN_POWER_LI_LIANG_GU_STRENGTH_DOWN_POWER.description");
}
