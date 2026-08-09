using GuZhenRen.Cards;
using GuZhenRen.Characters;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;

namespace GuZhenRen.Systems;

internal static class BenMingGuSelectionCoordinator
{
    private static bool _isSelecting;

    public static async Task TrySelect(AbstractRoom room)
    {
        if (_isSelecting || room is not EventRoom eventRoom)
        {
            return;
        }

        var player = eventRoom.LocalMutableEvent.Owner;
        if (player is null
            || player.Character is not FangYuanCharacter
            || player.Deck.Cards.OfType<AbstractBenMingGuCard>().Any())
        {
            return;
        }

        _isSelecting = true;
        try
        {
            var choices = GetChoices(player);
            var selectionScreen = NChooseACardSelectionScreen.ShowScreen(
                choices,
                false);
            var selected = selectionScreen is null
                ? choices[0]
                : (await selectionScreen.CardsSelected()).FirstOrDefault();

            if (selected is not null)
            {
                await AddToDeck(player, selected);
            }
        }
        finally
        {
            _isSelecting = false;
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

    private static async Task AddToDeck(Player player, CardModel selected)
    {
        if (selected is LiLiangGu or ZhiHuiGu)
        {
            var maxHpLoss = Math.Max(
                1,
                (int)Math.Floor(player.Creature.MaxHp * 0.33m));
            await CreatureCmd.LoseMaxHp(
                new ThrowingPlayerChoiceContext(),
                player.Creature,
                maxHpLoss,
                false);
        }

        var card = player.RunState.CreateCard(selected, player);
        card.FloorAddedToDeck = 1;
        SaveManager.Instance.MarkCardAsSeen(card);
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

        if (result.success && !RunManager.Instance.IsSingleplayerOrFakeMultiplayer)
        {
            RunManager.Instance.RewardSynchronizer.SyncLocalObtainedCard(card);
        }
    }
}
