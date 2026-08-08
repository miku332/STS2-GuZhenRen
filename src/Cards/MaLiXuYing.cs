using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class MaLiXuYing : AbstractXuYingCard
{
    protected override int ChancePercent => 20;

    protected override bool RequiresLiveTarget => false;

    public override int Rank => 2;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/MaLiXuYing.png");

    public MaLiXuYing()
        : base(CardType.Skill, TargetType.Self)
    {
    }

    protected override async Task TriggerXuYingEffect(
        PlayerChoiceContext choiceContext,
        CardPlay triggerCardPlay)
    {
        await CardPileCmd.Draw(choiceContext, 1, Owner);

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

        await CardCmd.Discard(choiceContext, selectedCards);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Chance"].UpgradeValueBy(15);
    }
}
