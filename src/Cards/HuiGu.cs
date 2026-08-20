using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Tags;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class HuiGu : GuZhenRenCardTemplate
{
    public override int Rank => IsUpgraded ? 8 : 7;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/HuiGu.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.ZhiDao];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(1)
    ];

    public HuiGu()
        : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        var exhaustPile = PileType.Exhaust.GetPile(Owner);
        if (exhaustPile.Cards.Count == 0)
        {
            return;
        }

        var amount = Math.Min(
            (int)DynamicVars["Cards"].BaseValue,
            exhaustPile.Cards.Count);
        var selected = exhaustPile.Cards.Count <= amount
            ? exhaustPile.Cards.ToList()
            : (await CardSelectCmd.FromSimpleGrid(
                choiceContext,
                exhaustPile.Cards,
                Owner,
                new CardSelectorPrefs(SelectionScreenPrompt, amount))).ToList();

        if (selected.Count == 0)
        {
            return;
        }

        foreach (var card in selected)
        {
            card.SetToFreeThisTurn();
        }

        if (CombatState is not null)
        {
            var regrets = selected
                .Where(card => card is HuiGu)
                .Select(_ => CombatState.CreateCard(ModelDb.Card<Regret>(), Owner))
                .ToList();
            foreach (var regret in regrets)
            {
                await CardPileCmd.AddGeneratedCardToCombat(
                    regret,
                    PileType.Hand,
                    Owner,
                    CardPilePosition.Bottom);
            }
        }

        await CardPileCmd.Add(
            selected,
            PileType.Hand,
            CardPilePosition.Bottom,
            null,
            false);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Cards"].UpgradeValueBy(1);
    }
}
