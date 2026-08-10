using GuZhenRen.CardPools;
using GuZhenRen.Keywords;
using GuZhenRen.Systems;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class YunSuan : GuZhenRenCardTemplate
{
    public override int Rank => IsUpgraded ? 7 : 6;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/YunSuan.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.ZhiDao];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [GuZhenRenKeywords.GaiLv];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(2),
        new DynamicVar("Increase", 10)
    ];

    public YunSuan()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        var hand = PileType.Hand.GetPile(Owner);
        var amount = Math.Min(
            hand.Cards.Count,
            DynamicVars["Cards"].IntValue);

        if (amount > 0)
        {
            var selected = (await CardSelectCmd.FromHand(
                choiceContext,
                Owner,
                new CardSelectorPrefs(SelectionScreenPrompt, amount),
                static _ => true,
                this)).ToList();

            foreach (var card in selected)
            {
                await CardCmd.Exhaust(choiceContext, card);
            }
        }

        await CardPileCmd.Draw(
            choiceContext,
            DynamicVars["Cards"].IntValue,
            Owner);

        ProbabilitySystem.IncreaseHandProbabilities(
            Owner,
            DynamicVars["Increase"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Increase"].UpgradeValueBy(5);
    }
}
