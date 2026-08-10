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
public sealed class XuePiaoLiu : AbstractShaZhaoCard
{
    public override int Rank => 6;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/XuePiaoLiu.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.XueDao];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<XuePiaoLiuPower>(1)
    ];

    public XuePiaoLiu()
        : base(1, CardType.Power, CardRarity.Token, TargetType.Self, false)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await PowerCmd.Apply<XuePiaoLiuPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["XuePiaoLiuPower"].BaseValue,
            Owner.Creature,
            this);
    }
}
