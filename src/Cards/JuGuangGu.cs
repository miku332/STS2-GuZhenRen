using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Powers;
using GuZhenRen.Tags;
using GuZhenRen.Keywords;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class JuGuangGu : GuZhenRenCardTemplate
{
    private const int CardEnergyCost = 1;
    private const CardType CardTypeValue = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Uncommon;
    private const TargetType CardTargetType = TargetType.Self;

    public override int Rank => 3;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/JuGuangGu.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.GuangDao];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<ShanYaoPower>(1).WithPowerTooltip(),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("CalculatedShanYao")
            .WithMultiplier(static (CardModel card, Creature? _) =>
            {
                var hand = PileType.Hand.GetPile(card.Owner);
                var handCount = hand.Cards.Count(
                    handCard => GuZhenRenTagRules.HasEffectiveTag(
                        handCard,
                        GuZhenRenTags.GuangDao));

                return handCount + (card.Pile?.Type == PileType.Hand ? 0 : 1);
            })
    ];

    public JuGuangGu()
        : base(CardEnergyCost, CardTypeValue, CardRarityValue, CardTargetType, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        var amount = ((CalculatedVar)DynamicVars["CalculatedShanYao"])
            .Calculate(cardPlay.Target);

        if (amount > 0)
        {
            await ShanYaoPower.Apply(
                choiceContext,
                cardPlay.Card.Owner.Creature,
                amount,
                cardPlay.Card.Owner.Creature,
                this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ShanYaoPower"].UpgradeValueBy(1);
        DynamicVars.CalculationExtra.UpgradeValueBy(1);
    }
}
