using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class HeiMangXuYing : AbstractXuYingCard
{
    protected override int ChancePercent => 25;

    protected override IEnumerable<DynamicVar> AdditionalVars =>
    [
        new PowerVar<ConstrictPower>(4)
    ];

    public override int Rank => 3;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/HeiMangXuYing.png");

    public HeiMangXuYing()
        : base(CardType.Skill, TargetType.AnyEnemy)
    {
    }

    protected override async Task TriggerXuYingEffect(
        PlayerChoiceContext choiceContext,
        CardPlay triggerCardPlay)
    {
        ArgumentNullException.ThrowIfNull(triggerCardPlay.Target);

        await PowerCmd.Apply<ConstrictPower>(
            choiceContext,
            triggerCardPlay.Target,
            DynamicVars["ConstrictPower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ConstrictPower"].UpgradeValueBy(2);
    }
}
