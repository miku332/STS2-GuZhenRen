using GuZhenRen.CardPools;
using GuZhenRen.Patches;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class FangWeiGu : GuZhenRenCardTemplate
{
    public override int Rank => IsUpgraded ? 8 : 7;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/FangWeiGu.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.BianHuaDao];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    public FangWeiGu()
        : base(2, CardType.Skill, CardRarity.Rare, TargetType.None, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(CombatState);

        var candidates = GetCandidates();
        if (candidates.Count == 0)
        {
            return;
        }

        var selected = (await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            candidates,
            Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 1))).FirstOrDefault();
        if (selected is null)
        {
            return;
        }

        var canonical = ModelDb.GetById<CardModel>(selected.Id);
        var copy = CombatState.CreateCard(canonical, Owner);
        using var uniquenessBypass =
            BenMingGuUniquenessPatch.EnterCombatBypassScope();
        await CardPileCmd.AddGeneratedCardToCombat(
            copy,
            PileType.Hand,
            Owner,
            CardPilePosition.Bottom);
    }

    protected override void OnUpgrade()
    {
    }

    private List<CardModel> GetCandidates()
    {
        IEnumerable<CardModel> cards = IsUpgraded
            ? ModelDb.AllCards
                .OfType<GuZhenRenCardTemplate>()
                .Where(IsEligible)
                .Select(card => (CardModel)card)
            : Owner.Deck.Cards
                .OfType<GuZhenRenCardTemplate>()
                .Where(IsEligible)
                .Select(card => ModelDb.GetById<CardModel>(card.Id));

        return cards
            .DistinctBy(card => card.Id)
            .Select(card => CombatState!.CreateCard(card, Owner))
            .ToList();
    }

    private bool IsEligible(CardModel card)
    {
        return card.Id != Id
            && card is not AbstractBenMingGuCard
            && card is not AbstractXuYingCard
            && card is GuZhenRenCardTemplate guCard
            && guCard.Rank is >= 1 and <= 9
            && card.CanBeGeneratedInCombat;
    }
}
