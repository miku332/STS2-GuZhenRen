using System.Collections.Generic;
using GuZhenRen.CardPools;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class CunGuangYin : ModRelicTemplate
{
    private const string ExtraSmithKey = "ExtraSmith";

    public override RelicRarity Rarity => RelicRarity.Rare;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(ExtraSmithKey, 1)
    ];

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/relics/CunGuangYin.png",
        IconOutlinePath: "res://GuZhenRen/images/relics/outline/CunGuangYin.png",
        BigIconPath: "res://GuZhenRen/images/relics/CunGuangYin.png");
}
