using GuZhenRen.CardPools;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class XianQiao8 : AbstractKongQiaoRelic
{
    public override int Rank => 8;

    protected override int NeededXp => 3;

    protected override string RelicImageName => "XianQiao_8";

    protected override RelicModel? NextStage => ModelDb.Relic<XianQiao9>();
}
