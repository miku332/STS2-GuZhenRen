using GuZhenRen.CardPools;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class OptionRuTuWeiAnAnTuZhongShanBao
    : GuZhenRenCardTemplate
{
    public override int Rank => 0;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/AnTuZhongShanBao.png");

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Unplayable];

    public override bool CanBeGeneratedInCombat => false;

    public override bool CanBeGeneratedByModifiers => false;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("HuaShi", 3)];

    public OptionRuTuWeiAnAnTuZhongShanBao()
        : base(-2, CardType.Skill, CardRarity.Token, TargetType.None, false)
    {
    }

    protected override Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay) => Task.CompletedTask;

    protected override void OnUpgrade()
    {
    }
}
