using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Powers;
using GuZhenRen.Tags;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class HuoGu : GuZhenRenCardTemplate
{
    public override int Rank => IsUpgraded ? 2 : 1;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/HuoGu.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.YanDao];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        IsUpgraded ? [] : [CardKeyword.Ethereal];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(3),
        new PowerVar<FenShaoPower>(1)
    ];

    public HuoGu()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        var hand = PileType.Hand.GetPile(Owner);
        if (hand.Cards.Count == 0)
        {
            return;
        }

        var maxSelect = Math.Min(
            hand.Cards.Count,
            (int)DynamicVars["Cards"].BaseValue);
        var selectorPrefs = new CardSelectorPrefs(
            SelectionScreenPrompt,
            0,
            maxSelect)
        {
            Cancelable = true
        };
        var selectedCards = (await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            selectorPrefs,
            static _ => true,
            this)).ToList();

        foreach (var selected in selectedCards)
        {
            await CardCmd.Exhaust(choiceContext, selected);
            await PowerCmd.Apply<FenShaoPower>(
                choiceContext,
                cardPlay.Target,
                DynamicVars["FenShaoPower"].BaseValue,
                Owner.Creature,
                this);
        }
    }

    protected override void OnUpgrade()
    {
    }
}
