using GuZhenRen.CardPools;
using GuZhenRen.Powers;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class WanWuDaTongBian : AbstractShaZhaoCard
{
    public override int Rank => 0;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/WanWuDaTongBian.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.BianHuaDao];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<WanWuDaTongBianPower>(1),
        new PowerVar<BianHuaDaoDaoHenPower>(0).WithPowerTooltip()
    ];

    public WanWuDaTongBian()
        : base(1, CardType.Power, CardRarity.Token, TargetType.Self, false)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (Owner.Creature.GetPower<WanWuDaTongBianPower>() is not null)
        {
            return;
        }

        await PowerCmd.Apply<WanWuDaTongBianPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["WanWuDaTongBianPower"].BaseValue,
            Owner.Creature,
            this);
    }
}
