using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Networking.ManagedActions;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.Multiplayer;
using GuZhenRen.Systems;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class TongXinPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.None;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/TongXinPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/TongXinPower_p.png");

    public override Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        var affectedPlayer = GetAffectedPlayer(cardPlay.Card.Owner.Creature);
        if (!Owner.IsAlive
            || affectedPlayer is null
            || cardPlay.IsAutoPlay
            || cardPlay.Card.Owner.Creature != affectedPlayer)
        {
            return Task.CompletedTask;
        }

        if (affectedPlayer.Player is not { } player)
        {
            return Task.CompletedTask;
        }

        Flash();
        var candidates = PileType.Hand.GetPile(player).Cards
            .Where(IsCandidate)
            .ToList();
        var card = candidates.Count == 0
            ? null
            : player.RunState.Rng.CombatCardSelection.NextItem(candidates);
        if (card is null)
        {
            return Task.CompletedTask;
        }

        var target = GetTarget(card, player);
        if (card.TargetType == TargetType.AnyEnemy && target is null)
        {
            return Task.CompletedTask;
        }

        if (MultiplayerActionAuthority.IsAuthority)
        {
            var payload = new TongXinAutoPlayPayload(
                player.NetId,
                NetCombatCard.FromModel(card).CombatCardIndex,
                target?.CombatId);
            NetGuZhenRenActions.RequestWithRetry(
                () => NetGuZhenRenActions.RequestTongXin(payload),
                () => !player.Creature.IsDead
                    && card.Pile?.Type == PileType.Hand,
                static () => { },
                "TongXin");
        }
        return Task.CompletedTask;
    }

    private Creature? GetAffectedPlayer(Creature triggeringPlayer) =>
        Applier?.Player is not null
            ? Applier
            : Owner.CombatState?.Players.Count == 1
                ? Owner.CombatState.Players[0].Creature
                : triggeringPlayer.Player is not null
                    ? triggeringPlayer
                    : null;

    internal static async Task ExecuteManagedAutoPlayAsync(
        RitsuLibManagedNetActionContext<TongXinAutoPlayPayload> context)
    {
        var player = context.Player.RunState.Players
            .FirstOrDefault(candidate => candidate.NetId == context.Message.OwnerNetId);
        var card = NetCombatCard.ForTesting(context.Message.CardIndex).ToCardModelOrNull();
        if (player is null || card is null)
        {
            return;
        }

        if (CombatManager.Instance.IsOverOrEnding
            || player.Creature.IsDead
            || !IsCandidate(card))
        {
            return;
        }

        var target = card.CombatState?.HittableEnemies
            .FirstOrDefault(enemy => enemy.CombatId == context.Message.TargetCombatId);
        if (card.TargetType == TargetType.AnyEnemy && target is null)
        {
            return;
        }

        try
        {
            await card.SpendResources();
            await CardCmd.AutoPlay(
                context.PlayerChoiceContext,
                card,
                target,
                AutoPlayType.Default,
                skipXCapture: true,
                skipCardPileVisuals: false);
        }
        catch (Exception ex)
        {
            Entry.Logger.Info($"Failed to auto-play TongXin card '{card.Id}': {ex}");
        }
    }

    private static bool IsCandidate(CardModel card) =>
        card.Pile?.Type == PileType.Hand
        && !card.Keywords.Contains(CardKeyword.Unplayable)
        && card.EnergyCost.Canonical != -2
        && card.CanPlay();

    private static Creature? GetTarget(CardModel card, Player player)
    {
        if (card.TargetType != TargetType.AnyEnemy)
        {
            return null;
        }

        var enemies = card.CombatState?.HittableEnemies
            .Where(enemy => enemy.IsAlive)
            .ToList();
        return enemies is { Count: > 0 }
            ? player.RunState.Rng.CombatTargets.NextItem(enemies)
            : null;
    }
}
