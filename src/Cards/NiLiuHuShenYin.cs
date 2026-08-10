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
public sealed class NiLiuHuShenYin : AbstractShaZhaoCard
{
    public override int Rank => 0;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/NiLiuHuShenYin.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.LuDao];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<NiLiuHuShenYinPower>(1)
    ];

    public NiLiuHuShenYin()
        : base(2, CardType.Power, CardRarity.Token, TargetType.Self, false)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await PowerCmd.Apply<NiLiuHuShenYinPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["NiLiuHuShenYinPower"].BaseValue,
            Owner.Creature,
            this);
    }
}
