using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Powers;
using GuZhenRen.Tags;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class YanTongGu : GuZhenRenCardTemplate
{
    public override int Rank => IsUpgraded ? 5 : 4;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/YanTongGu.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.YanDao];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<YanTongPower>(1).WithPowerTooltip()
    ];

    protected override IEnumerable<GeneratedCardPreview> GeneratedCardPreviews =>
        [PreviewCard<Burn>()];

    public YanTongGu()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await PowerCmd.Apply<YanTongPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["YanTongPower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["YanTongPower"].UpgradeValueBy(1);
    }
}
