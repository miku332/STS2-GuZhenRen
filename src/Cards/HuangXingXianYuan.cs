using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class HuangXingXianYuan : GuZhenRenCardTemplate
{
    private const int MaxHandFillDraw = 10;

    public override int Rank => 0;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/HuangXingXianYuan.png");

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Retain,
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Energy", 5)
    ];

    public HuangXingXianYuan()
        : base(0, CardType.Skill, CardRarity.Token, TargetType.Self, false)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await PlayerCmd.GainEnergy(
            (int)DynamicVars["Energy"].BaseValue,
            Owner);

        await CardPileCmd.Draw(
            choiceContext,
            MaxHandFillDraw,
            Owner);
    }
}
