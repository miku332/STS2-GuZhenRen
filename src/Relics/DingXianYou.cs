using GuZhenRen.CardPools;
using MegaCrit.Sts2.Core.Entities.Relics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class DingXianYou : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Ancient;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/relics/DingXianYou.png",
        IconOutlinePath: "res://GuZhenRen/images/relics/outline/DingXianYou.png",
        BigIconPath: "res://GuZhenRen/images/relics/DingXianYou.png");

    public override bool ShouldAllowFreeTravel() => true;
}
