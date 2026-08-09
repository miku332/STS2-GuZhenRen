using GuZhenRen.CardPools;
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
public sealed class ChengGongGu : GuZhenRenCardTemplate
{
    public override int Rank => 9;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/ChengGongGu.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.LuDao];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    public override bool CanBeGeneratedInCombat => false;

    public override bool CanBeGeneratedByModifiers => false;

    public ChengGongGu()
        : base(0, CardType.Skill, CardRarity.Rare, TargetType.None, false)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(CombatState);

        var choices = new List<CardModel>
        {
            CombatState.CreateCard<OptionCaiFu>(Owner),
            CombatState.CreateCard<OptionYongSheng>(Owner),
            CombatState.CreateCard<OptionZiYou>(Owner)
        };
        var selected = (await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            choices,
            Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 1))).FirstOrDefault();

        switch (selected)
        {
            case OptionCaiFu:
                await PlayerCmd.GainGold(300m, Owner);
                break;

            case OptionYongSheng:
                await CreatureCmd.GainMaxHp(Owner.Creature, 16m);
                break;

            case OptionZiYou:
                await RemoveCardsFromDeck();
                break;
        }
    }

    private async Task RemoveCardsFromDeck()
    {
        var removableCount = Owner.Deck.Cards.Count(card => card.IsRemovable);
        var amount = Math.Min(2, removableCount);
        if (amount == 0)
        {
            return;
        }

        var selected = (await CardSelectCmd.FromDeckForRemoval(
            Owner,
            new CardSelectorPrefs(
                CardSelectorPrefs.RemoveSelectionPrompt,
                amount))).ToList();
        if (selected.Count > 0)
        {
            await CardPileCmd.RemoveFromDeck(selected, showPreview: true);
        }
    }

    protected override void OnUpgrade()
    {
    }
}
