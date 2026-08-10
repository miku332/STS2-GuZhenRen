using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Tags;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class YanZhouGu : GuZhenRenCardTemplate
{
    public override int Rank => IsUpgraded ? 3 : 2;

    public override bool GainsBlock => true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/YanZhouGu.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.YanDao];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(12, ValueProp.Move)
    ];

    protected override IEnumerable<GeneratedCardPreview> GeneratedCardPreviews =>
        [PreviewCard<Burn>()];

    public YanZhouGu()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true)
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
            cardPlay);

        var burn = Owner.Creature.CombatState!.CreateCard<Burn>(Owner);
        await CardPileCmd.AddGeneratedCardToCombat(
            burn,
            PileType.Hand,
            Owner,
            CardPilePosition.Bottom);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
    }
}
