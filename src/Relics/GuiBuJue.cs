using GuZhenRen.CardPools;
using MegaCrit.Sts2.Core.Entities.Relics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class GuiBuJue : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Event;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/relics/GuiBuJue.png",
        IconOutlinePath: "res://GuZhenRen/images/relics/outline/GuiBuJue.png",
        BigIconPath: "res://GuZhenRen/images/relics/GuiBuJue.png");
}
