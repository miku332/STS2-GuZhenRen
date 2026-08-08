using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Tags;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class HuoYuanGu : GuZhenRenCardTemplate
{
    public override int Rank => IsUpgraded ? 3 : 2;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/HuoYuanGu.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.YanDao];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(2)
    ];

    public HuoYuanGu()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        var hand = PileType.Hand.GetPile(Owner);
        if (hand.Cards.Count == 0)
        {
            return;
        }

        var selectedCards = await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 1),
            static _ => true,
            this);
        var selected = selectedCards.FirstOrDefault();
        if (selected is null)
        {
            return;
        }

        await CardCmd.Exhaust(choiceContext, selected);

        ArgumentNullException.ThrowIfNull(CombatState);

        for (var i = 0; i < (int)DynamicVars["Cards"].BaseValue; i++)
        {
            var huoShi = CombatState.CreateCard<HuoShi>(Owner);
            if (IsUpgraded)
            {
                huoShi.UpgradeInternal();
                huoShi.FinalizeUpgradeInternal();
            }

            await CardPileCmd.AddGeneratedCardToCombat(
                huoShi,
                PileType.Hand,
                Owner,
                CardPilePosition.Bottom);
        }
    }

    protected override void OnUpgrade()
    {
    }
}
