using GuZhenRen.Multiplayer;
using GuZhenRen.Systems;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Networking.ManagedActions;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class YingShengChongPower : ModPowerTemplate
{
    [HarmonyPatch(
        typeof(CardModel),
        nameof(CardModel.ShouldGlowRed),
        MethodType.Getter)]
    private static class TargetGlowPatch
    {
        [HarmonyPostfix]
        private static void Postfix(CardModel __instance, ref bool __result)
        {
            __result |= IsTargetCard(__instance);
        }
    }

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
        SetTarget(player.NetId, target, announce: false);
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
        Targets.Remove(this);
        InvokeDisplayAmountChanged();
        ClearTargetVisual(cardPlay.Card);
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

        power.SetTarget(affectedPlayer.NetId, target, announce: true);
        if (target is not null)
        {
            power.Flash();
            Entry.Logger.Info(
                $"[Tribulation:YingShengChong] Locked {target.Id.Entry} "
                + $"for player {affectedPlayer.NetId}.");
        }

        return Task.CompletedTask;
    }

    public override Task AfterRemoved(Creature oldOwner)
    {
        if (Targets.TryGetValue(this, out var target))
        {
            Targets.Remove(this);
            ClearTargetVisual(ResolveTargetCard(target));
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
        CardModel? card,
        bool announce)
    {
        uint? cardIndex = card is null
            ? null
            : NetCombatCard.FromModel(card).CombatCardIndex;
        var cardName = card?.Title ?? string.Empty;
        var changed = !Targets.TryGetValue(this, out var previous)
            || previous.AffectedPlayerNetId != affectedPlayerNetId
            || previous.CardIndex != cardIndex;

        Targets[this] = new TargetState(affectedPlayerNetId, cardIndex, cardName);
        InvokeDisplayAmountChanged();

        if (changed && previous is not null)
        {
            ClearTargetVisual(ResolveTargetCard(previous));
        }

        if (card is null)
        {
            return;
        }

        ShowTargetVisual(card);
        if (announce)
        {
            var line = new LocString(
                "powers",
                "GU_ZHEN_REN_POWER_YING_SHENG_CHONG_POWER.target_speak");
            line.Add("CardName", cardName);
            TalkCmd.Play(line, Owner, VfxColor.Purple, VfxDuration.Long);
        }
    }

    private static void ShowTargetVisual(CardModel card)
    {
        if (!LocalContext.IsMine(card)
            || card.Pile?.Type != PileType.Hand
            || NCard.FindOnTable(card) is not { } cardNode)
        {
            return;
        }

        cardNode.CardHighlight.Modulate = NCardHighlight.red;
        cardNode.CardHighlight.AnimShow();
    }

    private static void ClearTargetVisual(CardModel? card)
    {
        if (card is null
            || !LocalContext.IsMine(card)
            || NCard.FindOnTable(card) is not { } cardNode)
        {
            return;
        }

        if (card.ShouldGlowRed)
        {
            cardNode.CardHighlight.Modulate = NCardHighlight.red;
            cardNode.CardHighlight.AnimShow();
        }
        else if (card.ShouldGlowGold)
        {
            cardNode.CardHighlight.Modulate = NCardHighlight.gold;
            cardNode.CardHighlight.AnimShow();
        }
        else if (card.CanPlay())
        {
            cardNode.CardHighlight.Modulate = NCardHighlight.playableColor;
            cardNode.CardHighlight.AnimShow();
        }
        else
        {
            cardNode.CardHighlight.AnimHide();
        }
    }

    private static CardModel? ResolveTargetCard(TargetState target)
    {
        if (target.CardIndex is not { } cardIndex)
        {
            return null;
        }

        var card = NetCombatCard.ForTesting(cardIndex).ToCardModelOrNull();
        return card?.Owner.NetId == target.AffectedPlayerNetId ? card : null;
    }

    private static bool IsTargetCard(CardModel card)
    {
        if (card.Pile?.Type != PileType.Hand)
        {
            return false;
        }

        var cardIndex = NetCombatCard.FromModel(card).CombatCardIndex;
        return Targets.Values.Any(target =>
            target.AffectedPlayerNetId == card.Owner.NetId
            && target.CardIndex == cardIndex);
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
