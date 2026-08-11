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
        var affectedPlayer = GetAffectedPlayer();
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
        RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(
            new TongXinAutoPlayAction(player));
        return Task.CompletedTask;
    }

    private Creature? GetAffectedPlayer() =>
        Applier?.Player is not null
            ? Applier
            : Owner.CombatState?.Players.FirstOrDefault()?.Creature;

    private sealed class TongXinAutoPlayAction : GameAction
    {
        private readonly Player _player;

        public TongXinAutoPlayAction(Player player)
        {
            _player = player;
        }

        public override ulong OwnerId => _player.NetId;

        public override GameActionType ActionType => GameActionType.Combat;

        public override bool RecordableToReplay => false;

        protected override async Task ExecuteAction()
        {
            if (_player.Creature.IsDead
                || CombatManager.Instance.IsOverOrEnding)
            {
                return;
            }

            var candidates = PileType.Hand.GetPile(_player).Cards
                .Where(IsCandidate)
                .ToList();
            if (candidates.Count == 0)
            {
                return;
            }

            var card = _player.RunState.Rng.CombatCardSelection.NextItem(candidates);
            if (card is null)
            {
                return;
            }

            var target = GetTarget(card);
            if (card.TargetType == TargetType.AnyEnemy && target is null)
            {
                return;
            }

            try
            {
                // AutoPlay is free by default. Spend the normal cost first so
                // this follows the original TongXin power's cost behavior.
                await card.SpendResources();
                await CardCmd.AutoPlay(
                    new GameActionPlayerChoiceContext(this),
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

        private Creature? GetTarget(CardModel card)
        {
            if (card.TargetType != TargetType.AnyEnemy)
            {
                return null;
            }

            var enemies = card.CombatState?.HittableEnemies
                .Where(enemy => enemy.IsAlive)
                .ToList();
            return enemies is { Count: > 0 }
                ? _player.RunState.Rng.CombatTargets.NextItem(enemies)
                : null;
        }

        public override INetAction ToNetAction()
        {
            throw new NotSupportedException(
                "GuZhenRen TongXin autoplay actions are single-player only for now.");
        }
    }
}
