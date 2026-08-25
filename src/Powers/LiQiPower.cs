using GuZhenRen.Multiplayer;
using GuZhenRen.Systems;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Combat.HandSize;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Networking.ManagedActions;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.Cards;
using GuZhenRen.Patches;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class LiQiPower : ModPowerTemplate, IMaxHandSizeModifier
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/LiQiPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/LiQiPower_p.png");

    public int ModifyMaxHandSizeLate(Player player, int currentMaxHandSize)
    {
        if (!Owner.IsAlive || player.Creature != Owner)
        {
            return currentMaxHandSize;
        }

        var shadowsInHand = PileType.Hand.GetPile(player)
            .Cards
            .Count(static card => card is AbstractXuYingCard);

        return currentMaxHandSize
            + shadowsInHand
            + XuYingHandSizePatch.GetPendingAllowance(player);
    }

    public override Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (player != Owner.Player || Amount <= 0)
        {
            return Task.CompletedTask;
        }

        var triggers = new List<LiQiTriggerPayload>();
        for (var round = 0; round < Amount; round++)
        {
            var shadows = PileType.Hand.GetPile(player)
                .Cards
                .OfType<AbstractXuYingCard>()
                .ToList();

            if (shadows.Count == 0)
            {
                break;
            }

            foreach (var shadow in shadows)
            {
                var target = GetRandomLivingEnemy(shadow);
                if (target is null)
                {
                    break;
                }

                triggers.Add(new(
                    NetCombatCard.FromModel(shadow).CombatCardIndex,
                    target.CombatId));
            }
        }

        if (triggers.Count == 0)
        {
            return Task.CompletedTask;
        }

        if (!MultiplayerActionAuthority.IsAuthority)
        {
            return Task.CompletedTask;
        }

        var payload = new LiQiAutoPlayPayload(player.NetId, triggers.ToArray());
        NetGuZhenRenActions.RequestWithRetry(
            () => NetGuZhenRenActions.RequestLiQi(payload),
            () => Owner.IsAlive
                && !CombatManager.Instance.IsOverOrEnding,
            static () => { },
            "LiQi");

        return Task.CompletedTask;
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

    internal static async Task ExecuteManagedAutoPlayAsync(
        RitsuLibManagedNetActionContext<LiQiAutoPlayPayload> context)
    {
        var owner = context.Player.RunState.Players
            .FirstOrDefault(candidate => candidate.NetId == context.Message.OwnerNetId);
        if (owner is null)
        {
            return;
        }

        foreach (var trigger in context.Message.Triggers)
        {
            if (CombatManager.Instance.IsOverOrEnding)
            {
                break;
            }

            var shadow = NetCombatCard.ForTesting(trigger.CardIndex)
                .ToCardModelOrNull() as AbstractXuYingCard;
            if (shadow is null
                || shadow.Owner != owner
                || shadow.Pile?.Type != PileType.Hand)
            {
                continue;
            }

            var target = shadow.CombatState?.HittableEnemies
                .FirstOrDefault(enemy => enemy.CombatId == trigger.TargetCombatId);
            if (target is null || !target.IsAlive)
            {
                continue;
            }

            owner.Creature.GetPower<LiQiPower>()?.Flash();
            await shadow.TriggerFromLiQiPower(
                context.PlayerChoiceContext,
                target);
        }
    }
}
