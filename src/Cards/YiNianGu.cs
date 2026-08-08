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

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class YiNianGu : GuZhenRenCardTemplate
{
    public override int Rank => 4;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/YiNianGu.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(1),
        new PowerVar<NianPower>(3)
    ];

    public YiNianGu()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        var discardPile = PileType.Discard.GetPile(Owner);
        if (discardPile.Cards.Count > 0)
        {
            var maxSelect = Math.Min(
                (int)DynamicVars["Cards"].BaseValue,
                discardPile.Cards.Count);
            var selected = discardPile.Cards.Count <= maxSelect
                ? discardPile.Cards.ToList()
                : (await CardSelectCmd.FromSimpleGrid(
                    choiceContext,
                    discardPile.Cards,
                    Owner,
                    new CardSelectorPrefs(SelectionScreenPrompt, maxSelect))).ToList();

            if (selected.Count > 0)
            {
                await CardPileCmd.Add(
                    selected,
                    PileType.Draw,
                    CardPilePosition.Top,
                    null,
                    false);
            }
        }

        await PowerCmd.Apply<NianPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["NianPower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["NianPower"].UpgradeValueBy(2);
    }
}
