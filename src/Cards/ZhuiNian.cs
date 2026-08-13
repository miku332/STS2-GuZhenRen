using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
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
public sealed class ZhuiNian : GuZhenRenCardTemplate
{
    public override int Rank => 6;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/ZhuiNian.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.ZhiDao];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<NianPower>(0).WithPowerTooltip()];

    public ZhuiNian()
        : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        var discardPile = PileType.Discard.GetPile(Owner);
        if (discardPile.Cards.Count == 0)
        {
            return;
        }

        var selected = (await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            discardPile.Cards,
            Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 1))).ToList();
        if (selected.Count == 0)
        {
            return;
        }

        var card = selected[0];
        var nianGain = CalculateNianGain(card);
        if (card.EnergyCost.CostsX)
        {
            card.EnergyCost.CapturedXValue = Owner.PlayerCombatState?.Energy ?? 0;
        }

        card.SetToFreeThisTurn();
        card.ExhaustOnNextPlay = true;
        await CardCmd.AutoPlay(
            choiceContext,
            card,
            null,
            AutoPlayType.Default,
            card.EnergyCost.CostsX,
            false);

        if (nianGain > 0)
        {
            await PowerCmd.Apply<NianPower>(
                choiceContext,
                Owner.Creature,
                nianGain,
                Owner.Creature,
                this);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }

    private decimal CalculateNianGain(CardModel card)
    {
        var cardCost = card.EnergyCost.CostsX
            ? Owner.PlayerCombatState?.Energy ?? 0
            : (int)card.EnergyCost.GetWithModifiers(CostModifiers.All);
        return Math.Max(0, cardCost) * 2;
    }
}
