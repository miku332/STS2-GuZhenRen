using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Keywords;
using GuZhenRen.Tags;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class ShiGuiFuLiGu : GuZhenRenCardTemplate
{
    public override int Rank => IsUpgraded ? 4 : 3;

    public override bool GainsBlock => true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/ShiGuiFuLiGu.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.LiDao];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        GuZhenRenKeywords.XuYing,
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(11, ValueProp.Move)
    ];

    protected override IEnumerable<GeneratedCardPreview> GeneratedCardPreviews =>
        [PreviewCard<GuiLiXuYing>(IsUpgraded)];

    public ShiGuiFuLiGu()
        : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(CombatState);

        await CreatureCmd.GainBlock(
            Owner.Creature,
            DynamicVars.Block.BaseValue,
            ValueProp.Move,
            cardPlay,
            false);

        var shadow = CombatState.CreateCard<GuiLiXuYing>(Owner);
        if (IsUpgraded)
        {
            shadow.UpgradeInternal();
            shadow.FinalizeUpgradeInternal();
        }

        await CardPileCmd.AddGeneratedCardToCombat(
            shadow,
            PileType.Hand,
            Owner,
            CardPilePosition.Bottom);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
    }
}
