using GuZhenRen.CardPools;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class OptionJuanTuChongLaiAnTuZhongShanBao
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
    [
        new DynamicVar("Cards", 0),
        new DynamicVar("JiTu", 0)
    ];

    public OptionJuanTuChongLaiAnTuZhongShanBao()
        : base(-2, CardType.Skill, CardRarity.Token, TargetType.None, false)
    {
    }

    public void SetCardCount(int count)
    {
        var safeCount = Math.Max(0, count);
        DynamicVars["Cards"].BaseValue = safeCount;
        DynamicVars["JiTu"].BaseValue = safeCount * 3;
    }

    protected override Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay) => Task.CompletedTask;

    protected override void OnUpgrade()
    {
    }
}
