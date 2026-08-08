using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Tags;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class XueKuangGu : GuZhenRenCardTemplate
{
    private static readonly HashSet<CardModel> _bloodcrazedCards =
        new(ReferenceEqualityComparer.Instance);

    private static readonly HashSet<CardModel> _autoPlayingCards =
        new(ReferenceEqualityComparer.Instance);

    private static readonly HashSet<CardModel> _queuedAutoPlayCards =
        new(ReferenceEqualityComparer.Instance);

    private CardModel? _leftCard;
    private CardModel? _rightCard;

    public override int Rank => 4;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/XueKuangGu.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.XueDao];

    public XueKuangGu()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    public static void ClearCombatState()
    {
        _bloodcrazedCards.Clear();
        _autoPlayingCards.Clear();
        _queuedAutoPlayCards.Clear();
    }

    public static void RefreshCachedAdjacentCardsInHand(Player owner)
    {
        var hand = PileType.Hand.GetPile(owner);
        var cards = hand.Cards.ToList();
        for (var i = 0; i < cards.Count; i++)
        {
            if (cards[i] is not XueKuangGu xueKuangGu)
            {
                continue;
            }

            xueKuangGu._leftCard = i > 0 ? cards[i - 1] : null;
            xueKuangGu._rightCard = i < cards.Count - 1 ? cards[i + 1] : null;
        }
    }

    public void CacheAdjacentCardsFromHand()
    {
        var hand = PileType.Hand.GetPile(Owner);
        var cards = hand.Cards.ToList();
        var index = cards.IndexOf(this);
        if (index < 0)
        {
            return;
        }

        _leftCard = index > 0 ? cards[index - 1] : null;
        _rightCard = index < cards.Count - 1 ? cards[index + 1] : null;
    }

    public static void TryAutoPlayBloodcrazedCard(CardModel? card)
    {
        if (card is null
            || !_bloodcrazedCards.Contains(card)
            || card.Pile?.Type != PileType.Hand
            || !_queuedAutoPlayCards.Add(card))
        {
            return;
        }

        RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(
            new BloodcrazeAutoPlayAction(card.Owner, [card]));
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        CacheAdjacentCardsFromHand();
        return base.BeforeCardPlayed(cardPlay);
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        MarkBloodcrazedCard(_leftCard);
        MarkBloodcrazedCard(_rightCard);
        TryAutoPlayBloodcrazedCard(_leftCard);
        TryAutoPlayBloodcrazedCard(_rightCard);
        await Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }

    private static void MarkBloodcrazedCard(CardModel? card)
    {
        if (card is null || card.Pile?.Type != PileType.Hand)
        {
            return;
        }

        _bloodcrazedCards.Add(card);
    }

    private static async Task TryAutoPlayBloodcrazedCardAsync(
        PlayerChoiceContext choiceContext,
        CardModel? card)
    {
        if (card is null
            || !_bloodcrazedCards.Contains(card)
            || !_autoPlayingCards.Add(card)
            || card.Pile?.Type != PileType.Hand)
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

            await CreatureCmd.Damage(
                choiceContext,
                card.Owner.Creature,
                2,
                MegaCrit.Sts2.Core.ValueProps.ValueProp.Unblockable
                    | MegaCrit.Sts2.Core.ValueProps.ValueProp.Unpowered,
                card.Owner.Creature,
                card);

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
            Entry.Logger.Info($"Failed to auto-play bloodcrazed card '{card.Id}': {ex}");
        }
        finally
        {
            _autoPlayingCards.Remove(card);
            _queuedAutoPlayCards.Remove(card);
        }
    }

    private bool HasLivingEnemies()
    {
        return CombatState?.HittableEnemies.Any(enemy => enemy.IsAlive) == true;
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
        if (aliveEnemies is null || aliveEnemies.Count == 0)
        {
            return null;
        }

        return card.Owner.RunState.Rng.CombatTargets.NextItem(aliveEnemies);
    }

    private sealed class BloodcrazeAutoPlayAction : GameAction
    {
        private readonly Player _owner;
        private readonly IReadOnlyList<CardModel> _cardsToPlay;

        public BloodcrazeAutoPlayAction(
            Player owner,
            IReadOnlyList<CardModel> cardsToPlay)
        {
            _owner = owner;
            _cardsToPlay = cardsToPlay;
        }

        public override ulong OwnerId => _owner.NetId;

        public override GameActionType ActionType => GameActionType.Combat;

        public override bool RecordableToReplay => false;

        protected override async Task ExecuteAction()
        {
            var choiceContext = new GameActionPlayerChoiceContext(this);
            foreach (var card in _cardsToPlay)
            {
                if (CombatManager.Instance.IsOverOrEnding || !HasLivingEnemies(card))
                {
                    break;
                }

                await TryAutoPlayBloodcrazedCardAsync(choiceContext, card);
            }

            foreach (var card in _cardsToPlay)
            {
                _queuedAutoPlayCards.Remove(card);
            }
        }

        public override INetAction ToNetAction()
        {
            throw new NotSupportedException(
                "GuZhenRen bloodcraze autoplay actions are single-player only for now.");
        }

        private static bool HasLivingEnemies(CardModel card)
        {
            return card.CombatState?.HittableEnemies.Any(enemy => enemy.IsAlive) == true;
        }
    }
}
