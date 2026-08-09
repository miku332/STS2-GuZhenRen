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
public sealed class WuZuNiao : GuZhenRenCardTemplate
{
    public override int Rank => IsUpgraded ? 4 : 3;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/WuZuNiao.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<FeiXingPower>(1),
        new PowerVar<ZhenChiGaoFeiPower>(3)
    ];

    public WuZuNiao()
        : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await PowerCmd.Apply<FeiXingPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["FeiXingPower"].BaseValue,
            Owner.Creature,
            this);

        await PowerCmd.Apply<ZhenChiGaoFeiPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["ZhenChiGaoFeiPower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ZhenChiGaoFeiPower"].UpgradeValueBy(1);
    }
}
