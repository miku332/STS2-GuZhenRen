using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class NiuLiXuYing : AbstractXuYingCard
{
    protected override int ChancePercent => 25;

    protected override IEnumerable<DynamicVar> AdditionalVars =>
    [
        new DamageVar(4, ValueProp.Move)
    ];

    public override int Rank => 2;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/NiuLiXuYing.png");

    public NiuLiXuYing()
        : base(CardType.Attack, TargetType.AnyEnemy)
    {
    }

    protected override async Task TriggerXuYingEffect(
        PlayerChoiceContext choiceContext,
        CardPlay triggerCardPlay)
    {
        ArgumentNullException.ThrowIfNull(triggerCardPlay.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(triggerCardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Chance"].UpgradeValueBy(15);
    }
}
