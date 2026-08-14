using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GuZhenRen.CardPools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class SiXuRuDianGu : ModRelicTemplate
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(3, ValueProp.Unpowered)
    ];

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/relics/SiXuRuDianGu.png",
        IconOutlinePath: "res://GuZhenRen/images/relics/outline/SiXuRuDianGu.png",
        BigIconPath: "res://GuZhenRen/images/relics/SiXuRuDianGu.png");

    public async Task OnNianGained(PlayerChoiceContext choiceContext)
    {
        if (Owner.Creature.CombatState is null || !Owner.Creature.IsAlive)
        {
            return;
        }

        var enemies = Owner.Creature.CombatState.HittableEnemies.ToList();
        if (enemies.Count == 0)
        {
            return;
        }

        Flash();
        await CreatureCmd.Damage(
            choiceContext,
            enemies,
            DynamicVars.Damage.BaseValue,
            DynamicVars.Damage.Props,
            Owner.Creature,
            null,
            null);
    }
}
