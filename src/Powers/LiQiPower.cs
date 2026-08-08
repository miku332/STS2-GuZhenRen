using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.Cards;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class LiQiPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/LiQiPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/LiQiPower_p.png");

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (player != Owner.Player || Amount <= 0)
        {
            return;
        }

        for (var round = 0; round < Amount; round++)
        {
            var shadows = PileType.Hand.GetPile(player)
                .Cards
                .OfType<AbstractXuYingCard>()
                .ToList();

            if (shadows.Count == 0)
            {
                return;
            }

            foreach (var shadow in shadows)
            {
                var target = GetRandomLivingEnemy(shadow);
                if (target is null)
                {
                    return;
                }

                Flash();
                await shadow.TriggerFromLiQiPower(choiceContext, target);
            }
        }
    }

    private static MegaCrit.Sts2.Core.Entities.Creatures.Creature? GetRandomLivingEnemy(
        AbstractXuYingCard shadow)
    {
        var enemies = shadow.CombatState?.HittableEnemies
            .Where(enemy => enemy.IsAlive)
            .ToList();

        return enemies is { Count: > 0 }
            ? shadow.Owner.RunState.Rng.CombatTargets.NextItem(enemies)
            : null;
    }
}
