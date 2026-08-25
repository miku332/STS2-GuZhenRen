using GuZhenRen.Multiplayer;
using GuZhenRen.Systems;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Networking.ManagedActions;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class YingShengChongPower : ModPowerTemplate
{
    private sealed record TargetState(
        ulong AffectedPlayerNetId,
        uint? CardIndex,
        string CardName);

    private static readonly Dictionary<YingShengChongPower, TargetState>
        Targets = new(ReferenceEqualityComparer.Instance);

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.None;

    public override LocString Description
    {
        get
        {
            var description = base.Description;
            var cardName = Targets.TryGetValue(this, out var target)
                ? target.CardName
                : string.Empty;
            description.Add("CardName", cardName);
            description.Add("HasTarget", !string.IsNullOrEmpty(cardName));
            return description;
        }
    }

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/YingShengChongPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/YingShengChongPower_p.png");

    public override Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (!Owner.IsAlive || player.Creature != GetAffectedPlayer())
        {
            return Task.CompletedTask;
        }

        var cards = PileType.Hand.GetPile(player).Cards;
        var target = cards.Count == 0
            ? null
            : player.RunState.Rng.CombatCardSelection.NextItem(cards);

        if (!MultiplayerActionAuthority.IsAuthority)
        {
            return Task.CompletedTask;
        }

        var payload = new YingShengChongTargetPayload(
            Owner.CombatId,
            player.NetId,
            target is null
                ? null
                : NetCombatCard.FromModel(target).CombatCardIndex);
        SetTarget(
            player.NetId,
            target is null
                ? null
                : NetCombatCard.FromModel(target).CombatCardIndex,
            target?.Title ?? string.Empty);
        NetGuZhenRenActions.RequestWithRetry(
            () => NetGuZhenRenActions.RequestYingShengChongTarget(payload),
            () => Owner.IsAlive && player.Creature.IsAlive,
            () => Entry.Logger.Warn("YingShengChong target action was not queued."),
            "YingShengChong target");
        return Task.CompletedTask;
    }

    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (!Owner.IsAlive
            || !Targets.TryGetValue(this, out var target)
            || target.CardIndex is not { } targetCardIndex
            || cardPlay.Card.Owner.Creature != GetAffectedPlayer(target.AffectedPlayerNetId)
            || NetCombatCard.FromModel(cardPlay.Card).CombatCardIndex != targetCardIndex)
        {
            return Task.CompletedTask;
        }

        Flash();
        Entry.Logger.Info(
            $"[Tribulation:YingShengChong] Killed player for playing {cardPlay.Card.Id.Entry}.");
        if (!MultiplayerActionAuthority.IsAuthority)
        {
            return Task.CompletedTask;
        }

        var payload = new YingShengChongKillPayload(
            target.AffectedPlayerNetId);
        NetGuZhenRenActions.RequestWithRetry(
            () => NetGuZhenRenActions.RequestYingShengChongKill(payload),
            () => Owner.IsAlive && cardPlay.Card.Owner.Creature.IsAlive,
            () => Entry.Logger.Warn("YingShengChong kill action was not queued."),
            "YingShengChong kill");
        return Task.CompletedTask;
    }

    internal static Task ExecuteManagedTargetAsync(
        RitsuLibManagedNetActionContext<YingShengChongTargetPayload> context)
    {
        var combatState = context.Player.Creature.CombatState;
        var owner = combatState?.Enemies.FirstOrDefault(enemy =>
            enemy.CombatId == context.Message.PowerOwnerCombatId);
        var power = owner?.GetPower<YingShengChongPower>();
        var affectedPlayer = context.Player.RunState.Players.FirstOrDefault(player =>
            player.NetId == context.Message.AffectedPlayerNetId);
        if (power is null || affectedPlayer is null)
        {
            return Task.CompletedTask;
        }

        CardModel? target = null;
        if (context.Message.CardIndex is { } cardIndex)
        {
            target = NetCombatCard.ForTesting(cardIndex).ToCardModelOrNull();
            if (target?.Owner != affectedPlayer
                || target.Pile?.Type != PileType.Hand)
            {
                return Task.CompletedTask;
            }
        }

        power.SetTarget(
            affectedPlayer.NetId,
            context.Message.CardIndex,
            target?.Title ?? string.Empty);
        if (target is not null)
        {
            power.Flash();
            Entry.Logger.Info(
                $"[Tribulation:YingShengChong] Locked {target.Id.Entry} "
                + $"for player {affectedPlayer.NetId}.");
        }

        return Task.CompletedTask;
    }

    internal static async Task ExecuteManagedKillAsync(
        RitsuLibManagedNetActionContext<YingShengChongKillPayload> context)
    {
        var target = context.Player.RunState.Players.FirstOrDefault(player =>
            player.NetId == context.Message.AffectedPlayerNetId);
        if (target?.Creature is not { IsAlive: true } creature)
        {
            return;
        }

        Entry.Logger.Info(
            $"[Tribulation:YingShengChong] Executing synchronized kill for player "
            + $"{target.NetId}.");
        await CreatureCmd.Kill(creature);
    }

    private void SetTarget(
        ulong affectedPlayerNetId,
        uint? cardIndex,
        string cardName)
    {
        Targets[this] = new TargetState(affectedPlayerNetId, cardIndex, cardName);
        InvokeDisplayAmountChanged();
    }

    private Creature? GetAffectedPlayer() =>
        Applier?.Player is not null
            ? Applier
            : Owner.CombatState?.Players.Count == 1
                ? Owner.CombatState.Players[0].Creature
                : null;

    private Creature? GetAffectedPlayer(ulong affectedPlayerNetId)
    {
        var player = Owner.CombatState?.Players.FirstOrDefault(candidate =>
            candidate.NetId == affectedPlayerNetId);
        return player?.Creature;
    }
}
