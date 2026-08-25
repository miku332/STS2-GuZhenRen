using System.Threading.Tasks;
using GuZhenRen.CardPools;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class ChiXiang : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/relics/ChiXiang.png",
        IconOutlinePath: "res://GuZhenRen/images/relics/outline/ChiXiang.png",
        BigIconPath: "res://GuZhenRen/images/relics/ChiXiang.png");

    public static int GetFatalCounterReduction(Player player) =>
        player.GetRelic<ChiXiang>() is null ? 1 : 2;

    public override async Task AfterDeath(
        PlayerChoiceContext choiceContext,
        Creature creature,
        bool wasRemovalPrevented,
        float deathAnimLength)
    {
        var combatState = Owner.Creature.CombatState;
        if (wasRemovalPrevented
            || combatState is null
            || creature.IsSecondaryEnemy
            || !combatState.Enemies.Contains(creature)
            || creature.GetPower<MinionPower>() is not null
            || !Owner.Creature.IsAlive)
        {
            return;
        }

        Flash();
        await CreatureCmd.Heal(Owner.Creature, 1m);
    }
}
