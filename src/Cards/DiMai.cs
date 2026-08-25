using GuZhenRen.CardPools;
using GuZhenRen.Tags;
using GuZhenRen.Multiplayer;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Networking.ManagedActions;
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
        if (Owner.NetId != RunManager.Instance.NetService.NetId)
        {
            return;
        }

        var discardCount = PileType.Discard.GetPile(Owner).Cards.Count;
        var handCount = IsUpgraded
            ? PileType.Hand.GetPile(Owner).Cards.Count
            : 0;
        var block = discardCount + handCount;

        if (block > 0)
        {
            var payload = new DiMaiBlockPayload(Owner.NetId, block);
            NetGuZhenRenActions.RequestWithRetry(
                () => NetGuZhenRenActions.RequestDiMai(payload),
                () => !Owner.Creature.IsDead && Pile?.Type == PileType.Hand,
                static () => { },
                "DiMai");
        }
    }

    protected override Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay) =>
        Task.CompletedTask;

    protected override void OnUpgrade()
    {
    }

    internal static async Task ExecuteManagedBlockAsync(
        RitsuLibManagedNetActionContext<DiMaiBlockPayload> context)
    {
        var owner = context.Player.RunState.Players
            .FirstOrDefault(player => player.NetId == context.Message.TargetNetId);
        if (owner is null
            || owner.Creature.IsDead
            || context.Message.Block <= 0)
        {
            return;
        }

        await CreatureCmd.GainBlock(
            owner.Creature,
            context.Message.Block,
            MegaCrit.Sts2.Core.ValueProps.ValueProp.Move,
            null);
    }
}
