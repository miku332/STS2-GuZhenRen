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
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.Cards;
using GuZhenRen.Tags;

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

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (Owner.Player is null
            || cardPlay.Card.Owner.Creature != Owner
            || cardPlay.IsAutoPlay
            || Amount <= 0)
        {
            return;
        }

        if (ReferenceEquals(cardPlay.Card, _sourceCard))
        {
            _sourceCard = null;
            return;
        }

        Flash();
        SetAmount(Amount - 1, false);

        RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(
            new YiXinErYongAutoPlayAction(Owner.Player));

        if (Amount <= 0)
        {
            await PowerCmd.Remove(this);
        }
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

    private static async Task TryAutoPlayRandomCard(
        PlayerChoiceContext choiceContext,
        Player owner)
    {
        var candidates = PileType.Hand.GetPile(owner).Cards
            .Where(IsCandidateCard)
            .ToList();
        if (candidates.Count == 0)
        {
            return;
        }

        var card = owner.RunState.Rng.CombatCardSelection.NextItem(candidates);
        if (card is null)
        {
            return;
        }

        if (!AutoPlayingCards.Add(card))
        {
            return;
        }

        try
        {
            var target = GetAutoPlayTarget(card);
            if (card.TargetType == TargetType.AnyEnemy && target is null)
            {
                return;
            }

            if (card.EnergyCost.CostsX)
            {
                card.EnergyCost.CapturedXValue = card.Owner.PlayerCombatState?.Energy ?? 0;
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

    private static Creature? GetAutoPlayTarget(CardModel card)
    {
        if (card.TargetType != TargetType.AnyEnemy)
        {
            return null;
        }

        var aliveEnemies = card.CombatState?.HittableEnemies
            .Where(enemy => enemy.IsAlive)
            .ToList();
        return aliveEnemies is { Count: > 0 }
            ? card.Owner.RunState.Rng.CombatTargets.NextItem(aliveEnemies)
            : null;
    }

    private sealed class YiXinErYongAutoPlayAction : GameAction
    {
        private readonly Player _owner;

        public YiXinErYongAutoPlayAction(Player owner)
        {
            _owner = owner;
        }

        public override ulong OwnerId => _owner.NetId;

        public override GameActionType ActionType => GameActionType.Combat;

        public override bool RecordableToReplay => false;

        protected override async Task ExecuteAction()
        {
            if (CombatManager.Instance.IsOverOrEnding)
            {
                return;
            }

            await TryAutoPlayRandomCard(
                new GameActionPlayerChoiceContext(this),
                _owner);
        }

        public override INetAction ToNetAction()
        {
            throw new NotSupportedException(
                "GuZhenRen YiXinErYong autoplay actions are single-player only for now.");
        }
    }
}
