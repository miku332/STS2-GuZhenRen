using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Audio.Debug;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Networking.ManagedActions;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.Multiplayer;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class PaiNanPower : ModPowerTemplate
{
    private static readonly HashSet<CardModel> _queuedCards =
        new(ReferenceEqualityComparer.Instance);

    public static void ResetCombatState() => _queuedCards.Clear();

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/PaiNanPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/PaiNanPower_p.png");

    public static void TryHandleCardDrawn(CardModel card)
    {
        if (card.Owner.NetId != RunManager.Instance.NetService.NetId)
        {
            return;
        }

        var owner = card.Owner;
        var power = owner.Creature.GetPower<PaiNanPower>();
        if (power is null || power.Amount <= 0)
        {
            return;
        }

        if (card.Pile?.Type != PileType.Hand
            || !IsStatusForPaiNan(card)
            || !_queuedCards.Add(card))
        {
            return;
        }

        var handCount = PileType.Hand.GetPile(owner).Cards.Count;
        if (card.Pile?.Type == PileType.Hand)
        {
            handCount--;
        }

        var drawAmount = Math.Min(
            Math.Max(0, (int)power.Amount),
            Math.Max(0, CardPile.MaxCardsInHand - handCount));
        var cardsToDraw = PileType.Draw.GetPile(owner)
            .Cards
            .Take(drawAmount)
            .ToList();

        var triggerCard = NetCombatCard.FromModel(card);
        var payload = new PaiNanDrawPayload(
            owner.NetId,
            triggerCard.CombatCardIndex,
            drawAmount,
            cardsToDraw
                .Select(drawCard => NetCombatCard.FromModel(drawCard).CombatCardIndex)
                .ToArray());
        NetGuZhenRenActions.RequestWithRetry(
            () => NetGuZhenRenActions.RequestPaiNan(payload),
            () => !owner.Creature.IsDead
                && card.Pile?.Type == PileType.Hand,
            () => _queuedCards.Remove(card),
            "PaiNan");
    }

    private static bool IsStatusForPaiNan(CardModel card) =>
        card.Type == CardType.Status || card is Burn;

    internal static async Task ExecuteManagedDrawAsync(
        RitsuLibManagedNetActionContext<PaiNanDrawPayload> context)
    {
        var target = context.Player.RunState.Players
            .FirstOrDefault(player => player.NetId == context.Message.TargetNetId);
        if (target is null)
        {
            return;
        }

        var card = NetCombatCard.ForTesting(context.Message.TriggerCardIndex).ToCardModelOrNull();
        if (card is null)
        {
            return;
        }

        var cardsToDraw = context.Message.CardIndices
            .Select(NetCombatCard.ForTesting)
            .Select(netCard => netCard.ToCardModelOrNull())
            .Where(drawCard => drawCard is not null)
            .Cast<CardModel>()
            .ToList();

        await new PaiNanDrawExecutor(
            target,
            card,
            context.Message.DrawAmount,
            cardsToDraw).ExecuteAsync(context.PlayerChoiceContext);
    }

    private sealed class PaiNanDrawExecutor
    {
        private readonly Player _target;
        private readonly CardModel _card;
        private readonly int _drawAmount;
        private readonly IReadOnlyList<CardModel> _cardsToDraw;

        public PaiNanDrawExecutor(
            Player target,
            CardModel card,
            int drawAmount,
            IReadOnlyList<CardModel> cardsToDraw)
        {
            _target = target;
            _card = card;
            _drawAmount = drawAmount;
            _cardsToDraw = cardsToDraw;
        }

        public async Task ExecuteAsync(PlayerChoiceContext choiceContext)
        {
            try
            {
                if (!IsStatusForPaiNan(_card))
                {
                    return;
                }

                _target.Creature.GetPower<PaiNanPower>()?.Flash();
                if (_card.Pile?.Type == PileType.Hand)
                {
                    await CardCmd.Exhaust(choiceContext, _card);
                }

                if (_target.Creature.CombatState is not { } combatState
                    || CombatManager.Instance.IsOverOrEnding)
                {
                    return;
                }

                if (!Hook.ShouldDraw(combatState, _target, fromHandDraw: false, out var modifier))
                {
                    await Hook.AfterPreventingDraw(combatState, modifier!);
                    return;
                }

                var drawAmount = Math.Min(
                    Math.Max(0, _drawAmount),
                    Math.Max(
                        0,
                        CardPile.MaxCardsInHand
                        - PileType.Hand.GetPile(_target).Cards.Count));
                if (drawAmount <= 0)
                {
                    return;
                }

                var drawnCount = await DrawSpecificCards(
                    choiceContext,
                    _cardsToDraw,
                    drawAmount);
                while (drawnCount < drawAmount)
                {
                    await CardPileCmd.ShuffleIfNecessary(choiceContext, _target);
                    var remainingCards = PileType.Draw.GetPile(_target)
                        .Cards
                        .Where(card => card.Pile?.Type == PileType.Draw)
                        .Take(drawAmount - drawnCount)
                        .ToList();
                    if (remainingCards.Count == 0)
                    {
                        break;
                    }

                    var progress = await DrawSpecificCards(
                        choiceContext,
                        remainingCards,
                        drawAmount - drawnCount);
                    if (progress <= 0)
                    {
                        break;
                    }

                    drawnCount += progress;
                }
            }
            finally
            {
                _queuedCards.Remove(_card);
            }
        }

        private async Task<int> DrawSpecificCards(
            PlayerChoiceContext choiceContext,
            IEnumerable<CardModel> cards,
            int maxCount)
        {
            var drawnCount = 0;
            foreach (var card in cards.ToList())
            {
                if (drawnCount >= maxCount)
                {
                    break;
                }

                if (card.Pile?.Type != PileType.Draw
                    || PileType.Hand.GetPile(_target).Cards.Count >= CardPile.MaxCardsInHand)
                {
                    continue;
                }

                await CardPileCmd.Add(card, PileType.Hand);
                var combatState = _target.Creature.CombatState;
                if (combatState is null)
                {
                    return drawnCount;
                }

                CombatManager.Instance.History.CardDrawn(combatState, card, fromHandDraw: false);
                await Hook.AfterCardDrawn(
                    combatState,
                    choiceContext,
                    card,
                    fromHandDraw: false);
                card.InvokeDrawn();
                TryHandleCardDrawn(card);
                NDebugAudioManager.Instance?.Play("card_deal.mp3", 0.25f, PitchVariance.Small);
                drawnCount++;
            }

            return drawnCount;
        }
    }
}
