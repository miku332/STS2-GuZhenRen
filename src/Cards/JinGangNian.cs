using System.Linq;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Tags;
using GuZhenRen.Keywords;
using GuZhenRen.Powers;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class JinGangNian : GuZhenRenCardTemplate
{
    protected override bool HasEnergyCostX => true;

    public override int Rank => 6;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/JinGangNian.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.ZhiDao];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<NianPower>(0).WithPowerTooltip(),
        new CalculationBaseVar(6),
        new ExtraDamageVar(1),
        new CalculatedDamageVar(ValueProp.Move)
            .WithMultiplier(static (card, _) =>
                ((JinGangNian)card).CalculateNianGainedThisTurn())
    ];

    public JinGangNian()
        : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        var hitCount = ResolveEnergyXValue();
        if (IsUpgraded)
        {
            hitCount++;
        }

        if (hitCount <= 0)
        {
            return;
        }

        await DamageCmd.Attack(DynamicVars.CalculatedDamage)
            .WithHitCount(hitCount)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
    }

    private int CalculateNianGainedThisTurn()
    {
        if (CombatState is null)
        {
            return 0;
        }

        decimal total = 0;
        foreach (PowerReceivedEntry entry in CombatManager.Instance.History.Entries.OfType<PowerReceivedEntry>())
        {
            if (entry.HappenedThisTurn(CombatState)
                && entry.Amount > 0
                && entry.Power is NianPower
                && entry.Power.Owner == Owner.Creature)
            {
                total += entry.Amount;
            }
        }

        return (int)total;
    }
}
