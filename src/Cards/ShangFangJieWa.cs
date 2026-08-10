using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Tags;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class ShangFangJieWa : AbstractShaZhaoCard
{
    private const int BaseDamage = 10;

    public override int Rank => 0;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/ShangFangJieWa.png");

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
        new DynamicVar("StrengthMultiplier", 3)
    ];

    public ShangFangJieWa()
        : base(1, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy, false)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await RemoveDefenses(cardPlay.Target);

        await DamageCmd.Attack(DynamicVars.GetComputedValue("CalculatedDamage"))
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
    }

    private static async Task RemoveDefenses(Creature target)
    {
        if (target.Block > 0)
        {
            await CreatureCmd.LoseBlock(target, target.Block);
        }

        await RemovePowerIfPresent<IntangiblePower>(target);
        await RemovePowerIfPresent<BufferPower>(target);
        await RemovePowerIfPresent<BarricadePower>(target);
        await RemovePowerIfPresent<CurlUpPower>(target);
        await RemovePowerIfPresent<GuardedPower>(target);
        await RemovePowerIfPresent<HardenedShellPower>(target);
    }

    private static async Task RemovePowerIfPresent<T>(Creature target)
        where T : PowerModel
    {
        if (target.GetPower<T>() is { } power)
        {
            await PowerCmd.Remove(power);
        }
    }
}
