using GuZhenRen.CardPools;
using GuZhenRen.Keywords;
using GuZhenRen.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class RuiYiGu : GuZhenRenCardTemplate
{
    public override int Rank => IsUpgraded ? 4 : 3;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/RuiYiGu.png");

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [GuZhenRenKeywords.JianFeng];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<JianFengPower>(1)
    ];

    public RuiYiGu()
        : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await PowerCmd.Apply<JianFengPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["JianFengPower"].BaseValue,
            Owner.Creature,
            this);

        if (Owner.Creature.GetPower<RuiYiPower>() is null)
        {
            await PowerCmd.Apply<RuiYiPower>(
                choiceContext,
                Owner.Creature,
                1,
                Owner.Creature,
                this);
        }
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
