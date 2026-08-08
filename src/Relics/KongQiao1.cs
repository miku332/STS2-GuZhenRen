using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using GuZhenRen.CardPools;
using GuZhenRen.Characters;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
[RegisterCharacterStarterRelic(typeof(FangYuanCharacter), 1)]
public sealed class KongQiao1 : AbstractKongQiaoRelic
{
    public override int Rank => 1;

    protected override int NeededXp => 1;

    protected override string RelicImageName => "KongQiao_1";

    protected override RelicModel? NextStage => ModelDb.Relic<KongQiao2>();

}
