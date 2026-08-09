using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Powers;
using GuZhenRen.Tags;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class LongXiGu : GuZhenRenCardTemplate
{
    public override int Rank => 6;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/LongXiGu.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.BianHuaDao];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<FenShaoPower>(6)
    ];

    public LongXiGu()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy, true)
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

        var burnAmount = cardPlay.Target.GetPower<FenShaoPower>()?.Amount ?? 0;
        if (burnAmount <= 0)
        {
            return;
        }

        await PowerCmd.Apply<JianHenPower>(
            choiceContext,
            cardPlay.Target,
            burnAmount,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["FenShaoPower"].UpgradeValueBy(2);
    }
}
