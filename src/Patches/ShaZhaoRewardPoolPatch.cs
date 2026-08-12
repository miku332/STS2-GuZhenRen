using GuZhenRen.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Patching.Models;

namespace GuZhenRen.Patches;

public sealed class ShaZhaoRewardPoolPatch : IPatchMethod
{
    public static string PatchId => "sha_zhao_reward_pool";

    public static string Description =>
        "Exclude Sha Zhao cards and owned XianGu from ordinary card pools";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(
            typeof(CardCreationOptions),
            nameof(CardCreationOptions.GetPossibleCards),
            [typeof(Player)])
    ];

    public static void Postfix(Player player, ref IEnumerable<CardModel> __result)
    {
        __result = __result.Where(
            card => card is not AbstractShaZhaoCard
                && card is not ChengGongGu
                && !IsOwnedUniqueImmortalGu(player, card));
    }

    internal static bool IsOwnedUniqueImmortalGu(Player player, CardModel card) =>
        BenMingGuUniquenessPatch.IsUniqueImmortalGu(card)
        && player.Deck.Cards.Any(ownedCard => ownedCard.Id == card.Id
            && BenMingGuUniquenessPatch.IsUniqueImmortalGu(ownedCard));
}

public sealed class ShaZhaoMerchantPoolPatch : IPatchMethod
{
    public static string PatchId => "sha_zhao_merchant_pool";

    public static string Description =>
        "Exclude Sha Zhao cards from merchant card pools";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(
            typeof(CardFactory),
            nameof(CardFactory.CreateForMerchant),
            [
                typeof(Player),
                typeof(IEnumerable<CardModel>),
                typeof(CardType)
            ]),
        new ModPatchTarget(
            typeof(CardFactory),
            nameof(CardFactory.CreateForMerchant),
            [
                typeof(Player),
                typeof(IEnumerable<CardModel>),
                typeof(CardRarity)
            ])
    ];

    public static void Prefix(Player player, ref IEnumerable<CardModel> options)
    {
        options = options.Where(
            card => card is not AbstractShaZhaoCard
                && card is not ChengGongGu
                && !ShaZhaoRewardPoolPatch.IsOwnedUniqueImmortalGu(player, card));
    }
}

public sealed class XianGuCardRewardResultPatch : IPatchMethod
{
    public static string PatchId => "xian_gu_card_reward_result";

    public static string Description =>
        "Replace reward cards that upgrade into an already-owned XianGu";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(
            typeof(CardFactory),
            nameof(CardFactory.CreateForReward),
            [
                typeof(Player),
                typeof(int),
                typeof(CardCreationOptions)
            ])
    ];

    public static void Postfix(
        Player player,
        CardCreationOptions options,
        ref IEnumerable<CardCreationResult> __result)
    {
        if (options.Source != CardCreationSource.Encounter)
        {
            return;
        }

        var results = __result.ToList();
        var rewardIds = results
            .Select(static result => result.Card.Id)
            .ToHashSet();

        foreach (var result in results)
        {
            var card = result.Card;
            if (!card.IsUpgraded
                || !ShaZhaoRewardPoolPatch.IsOwnedUniqueImmortalGu(player, card))
            {
                continue;
            }

            rewardIds.Remove(card.Id);
            var replacement = FindReplacement(
                player,
                options,
                card,
                rewardIds);
            if (replacement is null)
            {
                rewardIds.Add(card.Id);
                Entry.Logger.Warn(
                    $"Could not replace duplicate XianGu reward: {card.Id.Entry}");
                continue;
            }

            Entry.Logger.Info(
                $"Replaced duplicate XianGu reward {card.Id.Entry} with {replacement.Id.Entry}.");
            result.ModifyCard(replacement);
            rewardIds.Add(replacement.Id);
        }

        __result = results;
    }

    private static CardModel? FindReplacement(
        Player player,
        CardCreationOptions options,
        CardModel original,
        IReadOnlySet<ModelId> rewardIds)
    {
        foreach (var rarity in GetReplacementRarities(original.Rarity))
        {
            var candidates = options
                .GetPossibleCards(player)
                .Where(card => card.Rarity == rarity
                    && !rewardIds.Contains(card.Id))
                .Where(card => IsValidUpgradedReplacement(player, card))
                .ToList();
            if (candidates.Count == 0)
            {
                continue;
            }

            var rng = options.RngOverride ?? player.PlayerRng.Rewards;
            var canonicalCard = rng.NextItem(candidates);
            if (canonicalCard is null)
            {
                continue;
            }

            var replacement = player.RunState.CreateCard(canonicalCard, player);
            replacement.UpgradeInternal();
            replacement.FinalizeUpgradeInternal();
            return replacement;
        }

        return null;
    }

    private static bool IsValidUpgradedReplacement(
        Player player,
        CardModel canonicalCard)
    {
        if (!canonicalCard.IsUpgradable)
        {
            return false;
        }

        var preview = player.RunState.CreateCard(canonicalCard, player);
        preview.UpgradeInternal();
        preview.FinalizeUpgradeInternal();
        return !ShaZhaoRewardPoolPatch.IsOwnedUniqueImmortalGu(player, preview);
    }

    private static IEnumerable<CardRarity> GetReplacementRarities(
        CardRarity rarity) =>
        rarity switch
        {
            CardRarity.Rare =>
                [CardRarity.Rare, CardRarity.Uncommon, CardRarity.Common],
            CardRarity.Uncommon =>
                [CardRarity.Uncommon, CardRarity.Common],
            _ => [rarity]
        };
}
