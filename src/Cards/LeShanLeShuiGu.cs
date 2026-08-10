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

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class LeShanLeShuiGu : GuZhenRenCardTemplate
{
    public override int Rank => IsUpgraded ? 7 : 6;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/LeShanLeShuiGu.png");

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        GuZhenRenKeywords.Nian,
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<YiPower>(8)
    ];

    public LeShanLeShuiGu()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await PowerCmd.Apply<YiPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["YiPower"].BaseValue,
            Owner.Creature,
            this);

        await PowerCmd.Apply<NianTouShouZuPower>(
            choiceContext,
            Owner.Creature,
            1,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["YiPower"].UpgradeValueBy(3);
    }
}
