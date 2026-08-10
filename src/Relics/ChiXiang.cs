using System.Threading.Tasks;
using GuZhenRen.CardPools;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using STS2RitsuLib;
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

    public static async Task AfterCreatureDied(CreatureDiedEvent evt)
    {
        if (evt.WasRemovalPrevented
            || evt.CombatState is null
            || evt.Creature.IsSecondaryEnemy
            || !evt.CombatState.Enemies.Contains(evt.Creature)
            || evt.Creature.GetPower<MinionPower>() is not null)
        {
            return;
        }

        foreach (var player in evt.CombatState.Players)
        {
            var relic = player.GetRelic<ChiXiang>();
            if (relic is null || !player.Creature.IsAlive)
            {
                continue;
            }

            relic.Flash();
            await CreatureCmd.Heal(player.Creature, 1m);
        }
    }
}
