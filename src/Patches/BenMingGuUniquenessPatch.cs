using GuZhenRen.Cards;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using GuZhenRen.Relics;
using STS2RitsuLib.Patching.Models;

namespace GuZhenRen.Patches;

public sealed class BenMingGuUniquenessPatch : IPatchMethod
{
    public static string PatchId => "ben-ming-gu-deck-uniqueness";

    public static string Description => "Enforces BenMingGu uniqueness in the deck and combat.";

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

        if (newBenMingGuCards.Count == 0)
        {
            return mutableResults;
        }

        var duplicates = deck.Type == PileType.Deck
            ? GetDeckDuplicates(deck, newBenMingGuCards)
            : GetCombatDuplicates(newBenMingGuCards);
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
        IReadOnlyList<CardModel> newCards)
    {
        var existingCount = deck.Cards.Count(card =>
            card is AbstractBenMingGuCard && !newCards.Contains(card));
        return existingCount >= 1 ? newCards : newCards.Skip(1);
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
            .OfType<AbstractBenMingGuCard>()
            .Where(card => card.Rank >= 6 && !IsCopy(card))
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

    private static bool IsCopy(CardModel card) =>
        card.IsClone ||
        card.IsDupe ||
        card.Tags.Contains(GuZhenRenTags.XuYing);

    private static async Task DestroyDuplicateCard(CardModel card)
    {
        if (card.Pile?.Type != PileType.Deck)
        {
            return;
        }

        var owner = card.Owner;
        var grantsXianGuCanHai = owner is not null
            && card is AbstractBenMingGuCard { Rank: >= 6 }
            && !CombatManager.Instance.IsInProgress;

        Entry.Logger.Info($"Destroyed duplicate BenMingGu card: {card.Id.Entry}");
        await CardPileCmd.RemoveFromDeck(card, showPreview: false);

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
