using System.Collections.Generic;
using System.Threading.Tasks;
using GuZhenRen.CardPools;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class RouBaiGu : ModRelicTemplate
{
    private const string HealKey = "Heal";
    private bool _lostHpThisCombat;

    public override RelicRarity Rarity => RelicRarity.Event;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new HealVar(6)
    ];

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/relics/RouBaiGu.png",
        IconOutlinePath: "res://GuZhenRen/images/relics/outline/RouBaiGu.png",
        BigIconPath: "res://GuZhenRen/images/relics/RouBaiGu.png");

    public override Task BeforeCombatStart()
    {
        _lostHpThisCombat = false;
        return Task.CompletedTask;
    }

    public override Task AfterCurrentHpChanged(Creature creature, decimal delta)
    {
        if (creature == Owner.Creature && delta < 0 && CombatManager.Instance.IsInProgress)
        {
            _lostHpThisCombat = true;
        }

        return Task.CompletedTask;
    }

    public override async Task AfterCombatVictoryEarly(CombatRoom _)
    {
        if (!_lostHpThisCombat || Owner.Creature.IsDead)
        {
            return;
        }

        _lostHpThisCombat = false;
        Flash();
        await CreatureCmd.Heal(Owner.Creature, DynamicVars[HealKey].BaseValue);
    }
}
