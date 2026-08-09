using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class HeiHuo : GuZhenRenCardTemplate
{
    public override int Rank => 0;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/HeiHuo.png");

    public override CardPoolModel VisualCardPool => ModelDb.CardPool<CurseCardPool>();

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Unplayable
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("TurnEndMaxHpLoss", 3),
        new DynamicVar("ExhaustMaxHpLoss", 8)
    ];

    public override bool HasTurnEndInHandEffect => true;

    public override bool CanBeGeneratedInCombat => false;

    public override bool CanBeGeneratedByModifiers => false;

    public HeiHuo()
        : base(-1, CardType.Curse, CardRarity.Curse, TargetType.None, false)
    {
    }

    protected override Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay) =>
        Task.CompletedTask;

    protected override async Task OnTurnEndInHand(
        PlayerChoiceContext choiceContext)
    {
        await CreatureCmd.LoseMaxHp(
            choiceContext,
            Owner.Creature,
            DynamicVars["TurnEndMaxHpLoss"].BaseValue,
            true);
    }

    public override async Task AfterCardExhausted(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool causedByEthereal)
    {
        if (card != this)
        {
            return;
        }

        await CreatureCmd.LoseMaxHp(
            choiceContext,
            Owner.Creature,
            DynamicVars["ExhaustMaxHpLoss"].BaseValue,
            true);
    }
}
