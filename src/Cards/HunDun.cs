using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class HunDun : GuZhenRenCardTemplate
{
    public override int Rank => 0;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/HunDun.png");

    public override CardPoolModel VisualCardPool => ModelDb.CardPool<CurseCardPool>();

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Unplayable
    ];

    public HunDun()
        : base(-1, CardType.Curse, CardRarity.Curse, TargetType.None, false)
    {
    }

    public override bool HasTurnEndInHandEffect => true;

    public override bool CanBeGeneratedInCombat => false;

    public override bool CanBeGeneratedByModifiers => false;

    protected override IEnumerable<GeneratedCardPreview> GeneratedCardPreviews =>
        [PreviewCard<HeiHuo>()];

    protected override Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay) =>
        Task.CompletedTask;

    protected override async Task OnTurnEndInHand(
        PlayerChoiceContext choiceContext)
    {
        await CreatureCmd.Kill(Owner.Creature);
    }

    public override async Task AfterCardExhausted(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool causedByEthereal)
    {
        if (card != this
            || CombatState is null)
        {
            return;
        }

        for (var i = 0; i < 3; i++)
        {
            var heiHuo = CombatState.CreateCard<HeiHuo>(Owner);
            await CardPileCmd.AddGeneratedCardToCombat(
                heiHuo,
                PileType.Hand,
                Owner,
                CardPilePosition.Bottom);
        }
    }
}
