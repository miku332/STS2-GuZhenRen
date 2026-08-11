using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Tags;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
[RegisterCharacterStarterCard(typeof(GuZhenRen.Characters.FangYuanCharacter), 4)]
public sealed class YuPiGu : GuZhenRenCardTemplate
{
    private const int CardEnergyCost = 1;
    private const CardType CardTypeValue = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Basic;
    private const TargetType CardTargetType = TargetType.Self;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/YuPiGu.png");

    public override IEnumerable<CardTag> Tags =>
        [CardTag.Defend, GuZhenRenTags.JinDao];

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(5, ValueProp.Move)
    ];

    public YuPiGu()
        : base(CardEnergyCost, CardTypeValue, CardRarityValue, CardTargetType, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(
            cardPlay.Card.Owner.Creature,
            DynamicVars.Block.BaseValue,
            ValueProp.Move,
            cardPlay,
            false);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
    }
}
