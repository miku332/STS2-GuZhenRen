using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class PaiNanPower : ModPowerTemplate
{
    private static readonly HashSet<CardModel> _queuedCards =
        new(ReferenceEqualityComparer.Instance);

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/PaiNanPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/PaiNanPower_p.png");

    public static void TryHandleCardDrawn(CardModel card)
    {
        var owner = card.Owner;
        var power = owner.Creature.GetPower<PaiNanPower>();
        if (power is null || power.Amount <= 0)
        {
            return;
        }

        if (!IsStatusForPaiNan(card) || !_queuedCards.Add(card))
        {
            return;
        }

        RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(
            new PaiNanDrawAction(owner, card));
    }

    private static bool IsStatusForPaiNan(CardModel card) =>
        card.Type == CardType.Status || card is Burn;

    private sealed class PaiNanDrawAction : GameAction
    {
        private readonly Player _owner;
        private readonly CardModel _card;

        public PaiNanDrawAction(Player owner, CardModel card)
        {
            _owner = owner;
            _card = card;
        }

        public override ulong OwnerId => _owner.NetId;

        public override GameActionType ActionType => GameActionType.Combat;

        public override bool RecordableToReplay => false;

        protected override async Task ExecuteAction()
        {
            try
            {
                var power = _owner.Creature.GetPower<PaiNanPower>();
                if (power is null
                    || power.Amount <= 0
                    || !IsStatusForPaiNan(_card)
                    || _card.Pile?.Type != PileType.Hand)
                {
                    return;
                }

                power.Flash();
                var drawAmount = (int)power.Amount;
                var choiceContext = new GameActionPlayerChoiceContext(this);
                await CardCmd.Exhaust(choiceContext, _card);
                await CardPileCmd.Draw(choiceContext, drawAmount, _owner);
            }
            finally
            {
                _queuedCards.Remove(_card);
            }
        }

        public override INetAction ToNetAction()
        {
            throw new NotSupportedException(
                "GuZhenRen PaiNan draw actions are single-player only for now.");
        }
    }
}
