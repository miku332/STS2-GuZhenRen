using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Tags;
using GuZhenRen.Powers;
using GuZhenRen.Keywords;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class XingNianGu : GuZhenRenCardTemplate
{
    public override int Rank => 5;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/XingNianGu.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.ZhiDao];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(2),
        new PowerVar<NianPower>(3).WithPowerTooltip()
    ];

    public XingNianGu()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        var drawPile = PileType.Draw.GetPile(Owner);
        var count = Math.Min(
            drawPile.Cards.Count,
            (int)DynamicVars["Cards"].BaseValue);
        var candidates = drawPile.Cards.Take(count).ToList();

        if (candidates.Count == 0)
        {
            return;
        }

        var selectorPrefs = new CardSelectorPrefs(
            SelectionScreenPrompt,
            0,
            candidates.Count)
        {
            Cancelable = true
        };
        var selected = (await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            candidates,
            Owner,
            selectorPrefs)).ToList();

        if (selected.Count == 0)
        {
            return;
        }

        await CardPileCmd.Add(
            selected,
            PileType.Discard,
            CardPilePosition.Bottom,
            null,
            false);

        await PowerCmd.Apply<NianPower>(
            choiceContext,
            Owner.Creature,
            selected.Count * DynamicVars["NianPower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Cards"].UpgradeValueBy(1);
    }
}
