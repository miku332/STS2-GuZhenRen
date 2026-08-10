using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Keywords;
using GuZhenRen.Powers;
using GuZhenRen.Tags;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class YangMangBeiHuoYi : GuZhenRenCardTemplate
{
    public override int Rank => 0;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/YangMangBeiHuoYi.png");

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [GuZhenRenKeywords.FenShao];

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.YanDao];

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(24, ValueProp.Move),
        new PowerVar<FenShaoPower>(1),
        new PowerVar<YangMangBeiHuoYiPower>(3)
    ];

    public YangMangBeiHuoYi()
        : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self, false)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(
            Owner.Creature,
            DynamicVars.Block.BaseValue,
            DynamicVars.Block.Props,
            cardPlay,
            false);

        await PowerCmd.Apply<YangMangBeiHuoYiPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["YangMangBeiHuoYiPower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
    }
}
