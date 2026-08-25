using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Networking.ManagedActions;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Multiplayer;
using GuZhenRen.Powers;
using GuZhenRen.Systems;
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
        if (RunManager.Instance.NetService.Type == NetGameType.Singleplayer)
        {
            await TriggerDirectlyFromExhaustPile(choiceContext, player);
            return;
        }

        var triggers = BuildMultiplayerTriggers(player);
        if (triggers.Count == 0
            || !MultiplayerActionAuthority.IsAuthority)
        {
            return;
        }

        var payload = new RanNianFeiShiAutoPlayPayload(
            player.NetId,
            triggers.ToArray());
        NetGuZhenRenActions.RequestWithRetry(
            () => NetGuZhenRenActions.RequestRanNianFeiShi(payload),
            () => player.Creature.IsAlive
                && player.Creature.CombatState is not null
                && !CombatManager.Instance.IsOverOrEnding,
            static () => { },
            "RanNianFeiShi");
    }

    private static async Task TriggerDirectlyFromExhaustPile(
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

    private static List<RanNianFeiShiTriggerPayload> BuildMultiplayerTriggers(
        Player player)
    {
        var exhaustPile = PileType.Exhaust.GetPile(player);
        var combatState = player.Creature.CombatState;
        if (combatState is null)
        {
            return [];
        }

        var targets = combatState.HittableEnemies
            .Where(static enemy => enemy.IsAlive)
            .ToList();
        if (targets.Count == 0)
        {
            return [];
        }

        var triggers = new List<RanNianFeiShiTriggerPayload>();
        foreach (var card in exhaustPile.Cards.OfType<RanNianFeiShi>().ToList())
        {
            var target = player.RunState.Rng.CombatTargets.NextItem(targets);
            if (target is null)
            {
                break;
            }

            triggers.Add(new(
                NetCombatCard.FromModel(card).CombatCardIndex,
                target.CombatId));
        }

        return triggers;
    }

    internal static async Task ExecuteManagedAutoPlayAsync(
        RitsuLibManagedNetActionContext<RanNianFeiShiAutoPlayPayload> context)
    {
        var player = context.Player.RunState.Players
            .FirstOrDefault(candidate => candidate.NetId == context.Message.OwnerNetId);
        if (player is null)
        {
            return;
        }

        foreach (var trigger in context.Message.Triggers)
        {
            if (CombatManager.Instance.IsOverOrEnding
                || !player.Creature.IsAlive)
            {
                break;
            }

            var card = NetCombatCard.ForTesting(trigger.CardIndex)
                .ToCardModelOrNull() as RanNianFeiShi;
            if (card is null
                || card.Owner != player
                || card.Pile?.Type != PileType.Exhaust)
            {
                continue;
            }

            var target = card.CombatState?.HittableEnemies
                .FirstOrDefault(enemy => enemy.CombatId == trigger.TargetCombatId);
            if (target is null || !target.IsAlive)
            {
                continue;
            }

            await card.TriggerEffect(
                context.PlayerChoiceContext,
                target);
        }
    }

    private async Task TriggerEffect(
        PlayerChoiceContext choiceContext,
        Creature target)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
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
