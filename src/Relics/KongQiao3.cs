using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using GuZhenRen.CardPools;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class KongQiao3 : AbstractKongQiaoRelic
{
    public override int Rank => 3;

    protected override int NeededXp => 3;

    protected override string RelicImageName => "KongQiao_3";

    protected override RelicModel? NextStage => ModelDb.Relic<KongQiao4>();
}
