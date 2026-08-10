using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Keywords;
using GuZhenRen.Powers;
using GuZhenRen.Tags;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class GuangGu : GuZhenRenCardTemplate
{
    private const int CardEnergyCost = 1;
    private const CardType CardTypeValue = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Rare;
    private const TargetType CardTargetType = TargetType.Self;

    public override int Rank => 8;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/GuangGu.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.GuangDao];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [GuZhenRenKeywords.ShanYao];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<ShanYaoPower>(2),
        new PowerVar<RiGuangPower>(1)
    ];

    public GuangGu()
        : base(CardEnergyCost, CardTypeValue, CardRarityValue, CardTargetType, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        var owner = cardPlay.Card.Owner.Creature;

        await ShanYaoPower.Apply(
            choiceContext,
            owner,
            DynamicVars["ShanYaoPower"].BaseValue,
            owner,
            this);

        await PowerCmd.Apply<RiGuangPower>(
            choiceContext,
            owner,
            DynamicVars["RiGuangPower"].BaseValue,
            owner,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ShanYaoPower"].UpgradeValueBy(1);
    }
}
