using GuZhenRen.CardPools;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class DiMai : GuZhenRenCardTemplate
{
    public override int Rank => IsUpgraded ? 7 : 6;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/DiMai.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.TuDao];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Unplayable
    ];

    public DiMai()
        : base(-2, CardType.Skill, CardRarity.Rare, TargetType.None, true)
    {
    }

    public void KeepAtDrawPileBottom()
    {
        var drawPile = PileType.Draw.GetPile(Owner);
        if (drawPile.Cards.Contains(this))
        {
            drawPile.MoveToBottomInternal(this);
        }
    }

    public void OnCardDrawn()
    {
        var discardCount = PileType.Discard.GetPile(Owner).Cards.Count;
        var handCount = IsUpgraded
            ? PileType.Hand.GetPile(Owner).Cards.Count
            : 0;
        var block = discardCount + handCount;

        if (block > 0)
        {
            RunManager.Instance.ActionQueueSynchronizer.RequestEnqueue(
                new DiMaiBlockAction(Owner, block));
        }
    }

    protected override Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay) =>
        Task.CompletedTask;

    protected override void OnUpgrade()
    {
    }

    private sealed class DiMaiBlockAction : GameAction
    {
        private readonly Player _owner;
        private readonly int _block;

        public DiMaiBlockAction(Player owner, int block)
        {
            _owner = owner;
            _block = block;
        }

        public override ulong OwnerId => _owner.NetId;

        public override GameActionType ActionType => GameActionType.Combat;

        public override bool RecordableToReplay => false;

        protected override async Task ExecuteAction()
        {
            if (_owner.Creature.IsDead || _block <= 0)
            {
                return;
            }

            await CreatureCmd.GainBlock(
                _owner.Creature,
                _block,
                MegaCrit.Sts2.Core.ValueProps.ValueProp.Move,
                null);
        }

        public override INetAction ToNetAction()
        {
            throw new NotSupportedException(
                "GuZhenRen DiMai is single-player only for now.");
        }
    }
}
