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
public sealed class BianXing : AbstractBenMingGuCard
{
    protected override int MaxRank => 8;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/BianXing.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.BianHuaDao];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<BianHuaDaoDaoHenPower>(1).WithPowerTooltip()
    ];

    public BianXing()
        : base(1, CardType.Power, CardRarity.Token, TargetType.Self)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await PowerCmd.Apply<BianHuaDaoDaoHenPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["BianHuaDaoDaoHenPower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["BianHuaDaoDaoHenPower"].UpgradeValueBy(1);
    }
}
