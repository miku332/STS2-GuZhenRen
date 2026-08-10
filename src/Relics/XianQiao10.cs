using GuZhenRen.CardPools;
using GuZhenRen.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class XianQiao10 : AbstractKongQiaoRelic
{
    public override int Rank => 10;

    protected override int NeededXp => int.MaxValue;

    protected override string RelicImageName => "XianQiao_10";

    protected override RelicModel? NextStage => null;

    public override async Task AfterObtained()
    {
        await base.AfterObtained();
        await CreatureCmd.Heal(Owner.Creature, Owner.Creature.MaxHp);
    }

    public override async Task BeforeCombatStart()
    {
        await base.BeforeCombatStart();
        await PowerCmd.Apply<YongShengPower>(
            new ThrowingPlayerChoiceContext(),
            Owner.Creature,
            1,
            Owner.Creature,
            null);
    }
}
