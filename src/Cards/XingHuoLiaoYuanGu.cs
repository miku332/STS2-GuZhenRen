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
public sealed class XingHuoLiaoYuanGu : GuZhenRenCardTemplate
{
    public override int Rank => IsUpgraded ? 6 : 5;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/XingHuoLiaoYuanGu.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.YanDao];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        IsUpgraded
            ? [GuZhenRenKeywords.FenShao, CardKeyword.Exhaust, CardKeyword.Innate]
            : [GuZhenRenKeywords.FenShao, CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<FenShaoPower>(1)
    ];

    public XingHuoLiaoYuanGu()
        : base(2, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await PowerCmd.Apply<FenShaoPower>(
            choiceContext,
            cardPlay.Target,
            DynamicVars["FenShaoPower"].BaseValue,
            Owner.Creature,
            this);

        await PowerCmd.Apply<XingHuoLiaoYuanPower>(
            choiceContext,
            cardPlay.Target,
            1,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
    }
}
