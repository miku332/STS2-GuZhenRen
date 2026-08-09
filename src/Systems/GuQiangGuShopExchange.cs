using GuZhenRen.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace GuZhenRen.Systems;

public static class GuQiangGuShopExchange
{
    public static async Task ExchangeAll(IRunState runState)
    {
        foreach (var player in runState.Players)
        {
            var cards = player.Deck.Cards
                .Where(IsExchangeable)
                .ToList();
            var totalGold = cards.Sum(GetExchangeGold);

            foreach (var card in cards)
            {
                await CardPileCmd.RemoveFromDeck(card, showPreview: false);
            }

            if (totalGold > 0)
            {
                await PlayerCmd.GainGold(totalGold, player);
            }
        }
    }

    private static bool IsExchangeable(CardModel card) =>
        card is GuQiangGu or LuoXuanGuQiangGu;

    private static int GetExchangeGold(CardModel card) => card switch
    {
        GuQiangGu guQiangGu => guQiangGu.ShopExchangeGold,
        LuoXuanGuQiangGu luoXuan => luoXuan.ShopExchangeGold,
        _ => 0
    };
}
