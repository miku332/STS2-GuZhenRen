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
using GuZhenRen.Cards;
using GuZhenRen.Tags;
using GuZhenRen.Multiplayer;
using GuZhenRen.Systems;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class YiXinErYongPower : ModPowerTemplate
{
    private static readonly HashSet<CardModel> AutoPlayingCards =
        new(ReferenceEqualityComparer.Instance);

    private CardModel? _sourceCard;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/YiXinErYongPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/YiXinErYongPower_p.png");

    public override Task AfterApplied(
        Creature? applier,
        CardModel? cardSource)
    {
        _sourceCard = cardSource;
        return Task.CompletedTask;
    }

    public override Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (Owner.Player is null
            || cardPlay.Card.Owner.Creature != Owner
            || cardPlay.IsAutoPlay
            || Amount <= 0)
        {
            return Task.CompletedTask;
        }

        if (ReferenceEquals(cardPlay.Card, _sourceCard))
        {
            _sourceCard = null;
            return Task.CompletedTask;
        }

        var owner = Owner.Player;
        var candidates = PileType.Hand.GetPile(owner).Cards
            .Where(IsCandidateCard)
            .ToList();
        if (candidates.Count == 0)
        {
            return Task.CompletedTask;
        }

        var card = owner.RunState.Rng.CombatCardSelection.NextItem(candidates);
        if (card is null)
        {
            return Task.CompletedTask;
        }

        var target = GetAutoPlayTarget(card, owner);
        if (card.TargetType == TargetType.AnyEnemy && target is null)
        {
            return Task.CompletedTask;
        }

        if (MultiplayerActionAuthority.IsAuthority)
        {
            var payload = new YiXinErYongAutoPlayPayload(
                owner.NetId,
                NetCombatCard.FromModel(card).CombatCardIndex,
                target?.CombatId,
                card.EnergyCost.CostsX
                    ? card.Owner.PlayerCombatState?.Energy ?? 0
                    : 0);
            NetGuZhenRenActions.RequestWithRetry(
                () => NetGuZhenRenActions.RequestYiXinErYong(payload),
                () => !owner.Creature.IsDead
                    && card.Pile?.Type == PileType.Hand,
                static () => { },
                "YiXinErYong");
        }
        return Task.CompletedTask;
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner))
        {
            await PowerCmd.Remove(this);
        }
    }

    private static async Task TryAutoPlayCard(
        PlayerChoiceContext choiceContext,
        Player owner,
        CardModel card,
        Creature? target,
        int capturedXValue)
    {
        if (!AutoPlayingCards.Add(card))
        {
            return;
        }

        try
        {
            if (card.EnergyCost.CostsX)
            {
                card.EnergyCost.CapturedXValue = capturedXValue;
            }

            card.SetToFreeThisTurn();
            await CardCmd.AutoPlay(
                choiceContext,
                card,
                target,
                AutoPlayType.Default,
                card.EnergyCost.CostsX,
                false);
        }
        catch (Exception ex)
        {
            Entry.Logger.Info($"Failed to auto-play YiXinErYong card '{card.Id}': {ex}");
        }
        finally
        {
            AutoPlayingCards.Remove(card);
        }
    }

    private static bool IsCandidateCard(CardModel card)
    {
        return card.Pile?.Type == PileType.Hand
            && card is not AbstractXuYingCard
            && card is not JianYing
            && !card.Tags.Contains(GuZhenRenTags.XuYing)
            && !card.Keywords.Contains(CardKeyword.Unplayable);
    }

    private static Creature? GetAutoPlayTarget(CardModel card, Player owner)
    {
        if (card.TargetType != TargetType.AnyEnemy)
        {
            return null;
        }

        var aliveEnemies = card.CombatState?.HittableEnemies
            .Where(enemy => enemy.IsAlive)
            .ToList();
        return aliveEnemies is { Count: > 0 }
            ? owner.RunState.Rng.CombatTargets.NextItem(aliveEnemies)
            : null;
    }

    internal static async Task ExecuteManagedAutoPlayAsync(
        RitsuLibManagedNetActionContext<YiXinErYongAutoPlayPayload> context)
    {
        var owner = context.Player.RunState.Players
            .FirstOrDefault(player => player.NetId == context.Message.OwnerNetId);
        if (owner is null || CombatManager.Instance.IsOverOrEnding)
        {
            return;
        }

        var power = owner.Creature.GetPower<YiXinErYongPower>();
        if (power is null || power.Amount <= 0)
        {
            return;
        }

        power.Flash();
        power.SetAmount(power.Amount - 1, false);
        if (power.Amount <= 0)
        {
            await PowerCmd.Remove(power);
        }

        var card = NetCombatCard.ForTesting(context.Message.CardIndex).ToCardModelOrNull();
        if (card is null)
        {
            return;
        }

        var target = card.CombatState?.HittableEnemies
            .FirstOrDefault(enemy => enemy.CombatId == context.Message.TargetCombatId);
        if (card.TargetType == TargetType.AnyEnemy && target is null)
        {
            return;
        }

        await TryAutoPlayCard(
            context.PlayerChoiceContext,
            owner,
            card,
            target,
            context.Message.CapturedXValue);
    }
}
