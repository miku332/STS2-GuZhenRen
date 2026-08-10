using GuZhenRen.CardPools;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class XianQiao7 : AbstractKongQiaoRelic
{
    public override int Rank => 7;

    protected override int NeededXp => 2;

    protected override string RelicImageName => "XianQiao_7";

    protected override RelicModel? NextStage => ModelDb.Relic<XianQiao8>();
}
