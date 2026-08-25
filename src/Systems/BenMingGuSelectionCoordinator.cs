using System.Collections.Concurrent;
using Godot;
using GuZhenRen.Cards;
using GuZhenRen.Characters;
using GuZhenRen.Multiplayer;
using GuZhenRen.Patches;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using STS2RitsuLib.Networking.ManagedActions;

namespace GuZhenRen.Systems;

internal static class BenMingGuSelectionCoordinator
{
    private static readonly ConcurrentDictionary<ulong, byte> SelectingPlayers = [];
    private static readonly ConcurrentDictionary<ulong, byte> PendingSelections = [];
    private const double NetworkRetryDelaySeconds = 0.1;

    public static async Task TrySelect(AbstractRoom room)
    {
        if (room is not EventRoom eventRoom)
        {
            return;
        }

        var player = eventRoom.LocalMutableEvent.Owner;
        if (player is null
            || player.Character is not FangYuanCharacter
            || player.RunState.TotalFloor > 1
            || PendingSelections.ContainsKey(player.NetId)
            || player.Deck.Cards.OfType<AbstractBenMingGuCard>().Any())
        {
            return;
        }

        if (!SelectingPlayers.TryAdd(player.NetId, 0))
        {
            return;
        }

        NChooseACardSelectionScreen? selectionScreen = null;
        try
        {
            var choices = GetChoices(player);
            selectionScreen = NChooseACardSelectionScreen.ShowScreen(
                choices,
                false);
            if (selectionScreen is not null)
            {
                BenMingGuSelectionHeaderPatch.Refresh(
                    selectionScreen,
                    choices);
            }

            var selected = selectionScreen is null
                ? choices[0]
                : (await selectionScreen.CardsSelected()).FirstOrDefault();

            if (selected is not null)
            {
                QueueSelection(player, selected);
            }
        }
        finally
        {
            if (selectionScreen is not null)
            {
                BenMingGuSelectionHeaderPatch.Clear(selectionScreen);
            }

            SelectingPlayers.TryRemove(player.NetId, out _);
        }
    }

    private static List<CardModel> GetChoices(Player player)
    {
        List<CardModel> pool =
        [
            ModelDb.Card<BianXing>(),
            ModelDb.Card<HuoGu>(),
            ModelDb.Card<LiLiangGu>(),
            ModelDb.Card<RenGu>(),
            ModelDb.Card<ShaGu>(),
            ModelDb.Card<XinXue>(),
            ModelDb.Card<ZhiHuiGu>()
        ];

        var rng = new Rng(
            player.RunState.Rng.Seed,
            $"guzhenren_ben_ming_gu_{player.NetId}");
        rng.Shuffle(pool);
        return pool.Take(3).ToList();
    }

    private static void QueueSelection(Player player, CardModel selected)
    {
        if (!PendingSelections.TryAdd(player.NetId, 0))
        {
            return;
        }

        var payload = new BenMingGuSelectionPayload(selected.Id.ToString());
        TaskHelper.RunSafely(SynchronizeSelectionAsync(player, payload));
    }

    private static async Task SynchronizeSelectionAsync(
        Player player,
        BenMingGuSelectionPayload payload)
    {
        var tree = Engine.GetMainLoop() as SceneTree;
        if (tree is null)
        {
            PendingSelections.TryRemove(player.NetId, out _);
            Entry.Logger.Error(
                $"Failed to synchronize Ben Ming Gu selection for player {player.NetId}: no scene tree.");
            return;
        }

        var waitingForNetwork = false;
        while (RunManager.Instance.IsInProgress
               && player.Character is FangYuanCharacter
               && !player.Deck.Cards.OfType<AbstractBenMingGuCard>().Any())
        {
            if (NetGuZhenRenActions.RequestBenMingGuSelection(payload))
            {
                if (waitingForNetwork)
                {
                    Entry.Logger.Info(
                        $"Ben Ming Gu selection synchronization resumed for player {player.NetId}.");
                }

                return;
            }

            if (!waitingForNetwork)
            {
                waitingForNetwork = true;
                Entry.Logger.Info(
                    $"Waiting for multiplayer synchronization before applying Ben Ming Gu selection for player {player.NetId}.");
            }

            var timer = tree.CreateTimer(NetworkRetryDelaySeconds);
            await tree.ToSignal(timer, SceneTreeTimer.SignalName.Timeout);
        }

        PendingSelections.TryRemove(player.NetId, out _);
    }

    internal static async Task ExecuteManagedSelectionAsync(
        RitsuLibManagedNetActionContext<BenMingGuSelectionPayload> context)
    {
        var player = context.Player;
        try
        {
            if (player.Character is not FangYuanCharacter
                || player.Deck.Cards.OfType<AbstractBenMingGuCard>().Any())
            {
                return;
            }

            var selected = ModelDb.GetByIdOrNull<CardModel>(
                ModelId.Deserialize(context.Message.CardModelId));
            if (!IsInitialChoice(selected))
            {
                Entry.Logger.Warn(
                    $"Rejected invalid Ben Ming Gu selection '{context.Message.CardModelId}'.");
                return;
            }

            await AddToDeck(context.PlayerChoiceContext, player, selected!);
        }
        finally
        {
            PendingSelections.TryRemove(player.NetId, out _);
        }
    }

    private static bool IsInitialChoice(CardModel? card) =>
        card is BianXing
            or HuoGu
            or LiLiangGu
            or RenGu
            or ShaGu
            or XinXue
            or ZhiHuiGu;

    private static async Task AddToDeck(
        PlayerChoiceContext choiceContext,
        Player player,
        CardModel selected)
    {
        if (selected is LiLiangGu or ZhiHuiGu)
        {
            var maxHpLoss = Math.Max(
                1,
                (int)Math.Floor(player.Creature.MaxHp * 0.33m));
            await CreatureCmd.LoseMaxHp(
                choiceContext,
                player.Creature,
                maxHpLoss,
                false);
        }

        var card = player.RunState.CreateCard(selected, player);
        card.FloorAddedToDeck = 1;
        if (LocalContext.IsMe(player))
        {
            SaveManager.Instance.MarkCardAsSeen(card);
        }
        if (!player.DiscoveredCards.Contains(card.Id))
        {
            player.DiscoveredCards.Add(card.Id);
        }

        var result = await CardPileCmd.Add(
            card,
            PileType.Deck,
            CardPilePosition.Bottom,
            null,
            false);
        if (result.success)
        {
            result.cardAdded.Pile?.InvokeCardAddFinished();
        }

    }
}
