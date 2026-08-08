using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using GuZhenRen.CardPools;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class KongQiao4 : AbstractKongQiaoRelic
{
    public override int Rank => 4;

    protected override int NeededXp => 4;

    protected override string RelicImageName => "KongQiao_4";

    protected override RelicModel? NextStage => ModelDb.Relic<KongQiao5>();
}
