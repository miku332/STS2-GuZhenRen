using GuZhenRen.CardPools;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class XianQiao9 : AbstractKongQiaoRelic
{
    public override int Rank => 9;

    protected override int NeededXp => 0;

    protected override string RelicImageName => "XianQiao_9";

    protected override RelicModel? NextStage => null;
}
