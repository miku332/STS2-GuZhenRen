using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Keywords;
using GuZhenRen.Powers;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class WanXingFeiYing : AbstractShaZhaoCard
{
    public override int Rank => 0;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/WanXingFeiYing.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<WanXingFeiYingPower>(1),
        new PowerVar<NianPower>(0).WithPowerTooltip()
    ];

    public WanXingFeiYing()
        : base(1, CardType.Power, CardRarity.Token, TargetType.Self, false)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await PowerCmd.Apply<WanXingFeiYingPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["WanXingFeiYingPower"].BaseValue,
            Owner.Creature,
            this);
    }
}
