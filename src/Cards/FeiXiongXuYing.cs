using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class FeiXiongXuYing : AbstractXuYingCard
{
    private const int BaseDamage = 5;

    protected override int ChancePercent => 25;

    protected override bool RequiresLiveTarget => false;

    protected override IEnumerable<DynamicVar> AdditionalVars =>
    [
        ModCardVars.ComputedDamage(
            "CalculatedDamage",
            BaseDamage,
            static (card, _) =>
            {
                if (card is null)
                {
                    return BaseDamage;
                }

                var strength = card.Owner?.Creature.GetPowerAmount<StrengthPower>() ?? 0;
                var multiplier = card.DynamicVars["StrengthMultiplier"].BaseValue;
                return BaseDamage + strength * (multiplier - 1);
            },
            ValueProp.Move),
        new DynamicVar("StrengthMultiplier", 2)
    ];

    public override int Rank => 6;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/FeiXiongXuYing.png");

    public FeiXiongXuYing()
        : base(CardType.Attack, TargetType.AllEnemies)
    {
    }

    protected override async Task TriggerXuYingEffect(
        PlayerChoiceContext choiceContext,
        CardPlay triggerCardPlay)
    {
        ArgumentNullException.ThrowIfNull(CombatState);

        await DamageCmd.Attack(DynamicVars.GetComputedValue("CalculatedDamage"))
            .FromCard(this)
            .TargetingAllOpponents(CombatState)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["StrengthMultiplier"].UpgradeValueBy(1);
    }
}
