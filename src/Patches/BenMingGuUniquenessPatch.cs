using GuZhenRen.Cards;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using GuZhenRen.Relics;
using STS2RitsuLib.Patching.Models;

namespace GuZhenRen.Patches;

public sealed class BenMingGuUniquenessPatch : IPatchMethod
{
    internal static bool IsRemovingDuplicate { get; private set; }

    public static string PatchId => "ben-ming-gu-deck-uniqueness";

    public static string Description =>
        "Enforces BenMingGu and ordinary XianGu uniqueness in the deck and combat.";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(
            typeof(CardPileCmd),
            nameof(CardPileCmd.Add),
            [
                typeof(IEnumerable<CardModel>),
                typeof(CardPile),
                typeof(CardPilePosition),
                typeof(AbstractModel),
                typeof(bool)
            ])
    ];

    public static void Postfix(
        CardPile newPile,
        ref Task<IReadOnlyList<CardPileAddResult>> __result)
    {
        if (newPile.Type != PileType.Deck && !newPile.IsCombatPile)
        {
            return;
        }

        __result = RemoveDuplicateBenMingGuCards(__result, newPile);
    }

    private static async Task<IReadOnlyList<CardPileAddResult>> RemoveDuplicateBenMingGuCards(
        Task<IReadOnlyList<CardPileAddResult>> original,
        CardPile deck)
    {
        var results = await original;
        var mutableResults = results.ToList();
        var newBenMingGuCards = mutableResults
            .Where(result =>
                result.success &&
                result.oldPile is null &&
                result.cardAdded is AbstractBenMingGuCard benMingGu &&
                (deck.Type == PileType.Deck || benMingGu.Rank >= 6) &&
                !IsCopy(result.cardAdded))
            .Select(result => result.cardAdded)
            .ToList();

        var newXianGuCards = mutableResults
            .Where(result =>
                result.success &&
                result.oldPile is null &&
                IsUniqueImmortalGu(result.cardAdded) &&
                !IsCopy(result.cardAdded))
            .Select(result => result.cardAdded)
            .ToList();

        if (newBenMingGuCards.Count == 0 && newXianGuCards.Count == 0)
        {
            return mutableResults;
        }

        var duplicates = deck.Type == PileType.Deck
            ? GetDeckDuplicates(deck, newBenMingGuCards, newXianGuCards)
            : GetCombatDuplicates(newXianGuCards);
        foreach (var duplicate in duplicates)
        {
            if (deck.Type == PileType.Deck)
            {
                await DestroyDuplicateCard(duplicate);
            }
            else
            {
                await CardPileCmd.RemoveFromCombat(duplicate);
            }

            var resultIndex = mutableResults.FindIndex(
                result => result.cardAdded == duplicate);
            if (resultIndex >= 0)
            {
                var result = mutableResults[resultIndex];
                result.success = false;
                mutableResults[resultIndex] = result;
            }
        }

        return mutableResults;
    }

    private static IEnumerable<CardModel> GetDeckDuplicates(
        CardPile deck,
        IReadOnlyList<CardModel> newBenMingGuCards,
        IReadOnlyList<CardModel> newXianGuCards)
    {
        var duplicates = new List<CardModel>();
        var existingBenMingGu = deck.Cards.Any(card =>
            card is AbstractBenMingGuCard && !newBenMingGuCards.Contains(card));

        foreach (var card in newBenMingGuCards)
        {
            if (existingBenMingGu)
            {
                duplicates.Add(card);
            }
            else
            {
                existingBenMingGu = true;
            }
        }

        var existingXianGuIds = deck.Cards
            .Where(card => IsUniqueImmortalGu(card) && !newXianGuCards.Contains(card))
            .Select(card => card.Id)
            .ToHashSet();

        foreach (var card in newXianGuCards.Where(card =>
                     card is not AbstractBenMingGuCard))
        {
            if (!existingXianGuIds.Add(card.Id))
            {
                duplicates.Add(card);
            }
        }

        return duplicates.Distinct();
    }

    private static IEnumerable<CardModel> GetCombatDuplicates(
        IReadOnlyList<CardModel> newCards)
    {
        var existingIds = CardPile.GetCards(
                newCards[0].Owner,
                PileType.Hand,
                PileType.Draw,
                PileType.Discard,
                PileType.Exhaust,
                PileType.Play)
            .Where(card => !newCards.Contains(card))
            .Where(static card => IsUniqueImmortalGu(card))
            .Select(card => card.Id)
            .ToHashSet();

        var duplicates = new List<CardModel>();
        foreach (var card in newCards)
        {
            if (!existingIds.Add(card.Id))
            {
                duplicates.Add(card);
            }
        }

        return duplicates;
    }

    internal static bool IsCopy(CardModel card) =>
        card.IsClone ||
        card.IsDupe ||
        card.Tags.Contains(GuZhenRenTags.XuYing);

    internal static bool IsUniqueImmortalGu(
        CardModel card,
        int? rankOverride = null)
    {
        if (card is not GuZhenRenCardTemplate guCard)
        {
            return false;
        }

        var rank = rankOverride ?? guCard.Rank;
        return rank >= 6
            && card is not AbstractShaZhaoCard
            && (card is AbstractBenMingGuCard || card.Rarity != CardRarity.Token);
    }

    internal static async Task EnforceDeckUniqueness(Player owner)
    {
        var duplicates = new List<CardModel>();
        var foundBenMingGu = false;
        var xianGuIds = new HashSet<string>(StringComparer.Ordinal);

        foreach (var card in owner.Deck.Cards.ToList())
        {
            if (IsCopy(card))
            {
                continue;
            }

            if (card is AbstractBenMingGuCard)
            {
                if (foundBenMingGu)
                {
                    duplicates.Add(card);
                    continue;
                }

                foundBenMingGu = true;
            }

            if (card is not AbstractBenMingGuCard
                && IsUniqueImmortalGu(card)
                && !xianGuIds.Add(card.Id.Entry))
            {
                duplicates.Add(card);
            }
        }

        foreach (var duplicate in duplicates)
        {
            await DestroyDuplicateCard(duplicate);
        }
    }

    internal static async Task DestroyDuplicateCard(CardModel card)
    {
        if (card.Pile?.Type != PileType.Deck)
        {
            return;
        }

        var owner = card.Owner;
        var grantsXianGuCanHai = owner is not null
            && IsUniqueImmortalGu(card);

        Entry.Logger.Info($"Destroyed duplicate unique Gu card: {card.Id.Entry}");
        IsRemovingDuplicate = true;
        try
        {
            await CardPileCmd.RemoveFromDeck(card, showPreview: false);
        }
        finally
        {
            IsRemovingDuplicate = false;
        }

        if (!grantsXianGuCanHai || owner is null)
        {
            return;
        }

        var relic = owner.GetRelic<XianGuCanHai>();
        if (relic is null)
        {
            await RelicCmd.Obtain<XianGuCanHai>(owner);
        }
        else
        {
            relic.Counter++;
            relic.Flash();
        }
    }
}
