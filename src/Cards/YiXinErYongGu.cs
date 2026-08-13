using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Tags;
using GuZhenRen.Keywords;
using GuZhenRen.Powers;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class YiXinErYongGu : GuZhenRenCardTemplate
{
    public override int Rank => IsUpgraded ? 3 : 2;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/YiXinErYongGu.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.ZhiDao];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<NianPower>(3).WithPowerTooltip(),
        new PowerVar<YiXinErYongPower>(1)
    ];

    public YiXinErYongGu()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await PowerCmd.Apply<NianPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["NianPower"].BaseValue,
            Owner.Creature,
            this);

        await PowerCmd.Apply<YiXinErYongPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["YiXinErYongPower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["YiXinErYongPower"].UpgradeValueBy(1);
    }
}
