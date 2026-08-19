using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Powers;
using GuZhenRen.Tags;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class RanNianFeiShi : AbstractShaZhaoCard
{
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/RanNianFeiShi.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.ZhiDao];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(4, ValueProp.Move),
        new PowerVar<FenShaoPower>(4).WithPowerTooltip()
    ];

    public RanNianFeiShi()
        : base(0, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy, false)
    {
    }

    protected override Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        return TriggerEffect(choiceContext, cardPlay.Target);
    }

    internal static async Task TriggerFromExhaustPile(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        var exhaustPile = PileType.Exhaust.GetPile(player);
        var cards = exhaustPile.Cards.OfType<RanNianFeiShi>().ToList();

        foreach (var card in cards)
        {
            if (!player.Creature.IsAlive || !exhaustPile.Cards.Contains(card))
            {
                continue;
            }

            var combatState = player.Creature.CombatState;
            if (combatState is null)
            {
                break;
            }

            var targets = combatState.HittableEnemies
                .Where(static enemy => enemy.IsAlive)
                .ToList();
            var target = player.RunState.Rng.CombatTargets.NextItem(targets);
            if (target is null)
            {
                break;
            }

            await card.TriggerEffect(choiceContext, target);
        }
    }

    private async Task TriggerEffect(
        PlayerChoiceContext choiceContext,
        Creature target)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, null)
            .Targeting(target)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);

        if (!target.IsAlive)
        {
            return;
        }

        await PowerCmd.Apply<FenShaoPower>(
            choiceContext,
            target,
            DynamicVars["FenShaoPower"].BaseValue,
            Owner.Creature,
            this);
    }
}
