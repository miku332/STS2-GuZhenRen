using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Tags;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class WanWoDaShouYin : AbstractShaZhaoCard
{
    private const int BaseDamage = 24;

    public override int Rank => 0;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/WanWoDaShouYin.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.LiDao];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<StrengthPower>(0).WithPowerTooltip(),
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
        new DynamicVar("StrengthMultiplier", 8)
    ];

    public WanWoDaShouYin()
        : base(2, CardType.Attack, CardRarity.Token, TargetType.AllEnemies, false)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(CombatState);

        await DamageCmd.Attack(DynamicVars.GetComputedValue("CalculatedDamage"))
            .FromCard(this)
            .TargetingAllOpponents(CombatState)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
    }
}
