using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using GuZhenRen.CardPools;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class KongQiao2 : AbstractKongQiaoRelic
{
    public override int Rank => 2;

    protected override int NeededXp => 2;

    protected override string RelicImageName => "KongQiao_2";

    protected override RelicModel? NextStage => ModelDb.Relic<KongQiao3>();
}
