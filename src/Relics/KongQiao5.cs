using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using GuZhenRen.CardPools;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class KongQiao5 : AbstractKongQiaoRelic
{
    public override int Rank => 5;

    protected override int NeededXp => 5;

    protected override string RelicImageName => "KongQiao_5";

    protected override RelicModel? NextStage => null;
}
