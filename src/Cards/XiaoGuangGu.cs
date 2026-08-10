using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
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
[RegisterCharacterStarterCard(typeof(GuZhenRen.Characters.FangYuanCharacter), 1)]
public sealed class XiaoGuangGu : GuZhenRenCardTemplate
{
    private const int CardEnergyCost = 0;
    private const CardType CardTypeValue = CardType.Skill;
    private const CardRarity CardRarityValue = CardRarity.Basic;
    private const TargetType CardTargetType = TargetType.AnyEnemy;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/XiaoGuangGu.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.GuangDao];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [GuZhenRenKeywords.ShanYao];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<ShanYaoPower>(1),
        new PowerVar<WeakPower>(1)
    ];

    public XiaoGuangGu()
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

        await PowerCmd.Apply<WeakPower>(
            choiceContext,
            cardPlay.Target!,
            DynamicVars["WeakPower"].BaseValue,
            owner,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ShanYaoPower"].UpgradeValueBy(1);
    }
}
