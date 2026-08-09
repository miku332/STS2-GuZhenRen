using GuZhenRen.CardPools;
using GuZhenRen.Powers;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class ZhuanYun : GuZhenRenCardTemplate
{
    public override int Rank => IsUpgraded ? 7 : 6;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/ZhuanYun.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.YunDao];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<ZhuanYunPower>(4)
    ];

    public ZhuanYun()
        : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await PowerCmd.Apply<ZhuanYunPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["ZhuanYunPower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ZhuanYunPower"].UpgradeValueBy(2);
    }
}
