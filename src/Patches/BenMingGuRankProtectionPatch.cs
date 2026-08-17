using System.Reflection;
using GuZhenRen.Cards;
using GuZhenRen.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using STS2RitsuLib.Patching.Models;

namespace GuZhenRen.Patches;

internal static class BenMingGuRankProtection
{
    public static bool CanBePermanentlyDowngraded(CardModel card)
    {
        if (!card.IsUpgraded)
        {
            return false;
        }

        if (card is not AbstractBenMingGuCard benMingGu)
        {
            return true;
        }

        var minimumRank = card.Owner
            .GetRelic<AbstractKongQiaoRelic>()?.Rank ?? 1;
        return benMingGu.Rank > minimumRank;
    }

    public static void EnsureMinimumRank(Player player)
    {
        var minimumRank = player.GetRelic<AbstractKongQiaoRelic>()?.Rank ?? 1;

        foreach (var benMingGu in player.Deck.Cards
                     .OfType<AbstractBenMingGuCard>())
        {
            var originalRank = benMingGu.Rank;
            UpgradeInternallyToRank(benMingGu, minimumRank);

            if (benMingGu.Rank != originalRank)
            {
                Entry.Logger.Warn(
                    $"Restored BenMingGu '{benMingGu.Id.Entry}' from rank " +
                    $"{originalRank} to aperture rank {benMingGu.Rank}.");
            }
        }
    }

    public static void UpgradeInternallyToRank(
        AbstractBenMingGuCard benMingGu,
        int targetRank)
    {
        while (benMingGu.Rank < targetRank && benMingGu.IsUpgradable)
        {
            benMingGu.UpgradeInternal();
            benMingGu.FinalizeUpgradeInternal();
        }
    }
}

internal static class PermanentDowngradeEventPatchSupport
{
    private static readonly MethodInfo? SetEventFinishedMethod =
        typeof(EventModel).GetMethod(
            "SetEventFinished",
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            [typeof(LocString)],
            null);

    public static bool IsAvailable => SetEventFinishedMethod is not null;

    public static void Finish(EventModel eventModel, string descriptionKey)
    {
        SetEventFinishedMethod!.Invoke(
            eventModel,
            [new LocString(eventModel.LocTable, descriptionKey)]);
    }
}

public sealed class BenMingGuPersistentDowngradePatch : IPatchMethod
{
    public static string PatchId => "ben-ming-gu-rank-floor";

    public static string Description =>
        "Prevents persistent BenMingGu cards from being downgraded below the aperture rank.";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(
            typeof(CardCmd),
            nameof(CardCmd.Downgrade),
            [typeof(CardModel)])
    ];

    public static bool Prefix(CardModel card)
    {
        if (card is not AbstractBenMingGuCard benMingGu
            || card.Pile?.Type != PileType.Deck)
        {
            return true;
        }

        var minimumRank = card.Owner
            .GetRelic<AbstractKongQiaoRelic>()?.Rank ?? 1;

        if (benMingGu.Rank <= minimumRank)
        {
            Entry.Logger.Info(
                $"Prevented BenMingGu '{benMingGu.Id.Entry}' from being " +
                $"downgraded below aperture rank {minimumRank}.");
            return false;
        }

        card.Owner.RunState.CurrentMapPointHistoryEntry?
            .GetEntry(card.Owner.NetId)
            .DowngradedCards.Add(card.Id);
        benMingGu.DowngradeInternal();
        BenMingGuRankProtection.UpgradeInternallyToRank(
            benMingGu,
            minimumRank);

        Entry.Logger.Info(
            $"Downgraded BenMingGu '{benMingGu.Id.Entry}' to aperture " +
            $"rank floor {benMingGu.Rank}.");
        return false;
    }
}

public sealed class ReflectionsBenMingGuPatch : IPatchMethod
{
    public static string PatchId => "reflections-ben-ming-gu-rank-floor";

    public static string Description =>
        "Applies the aperture rank floor to BenMingGu downgrades in Reflections.";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(
            typeof(Reflections),
            "TouchAMirror",
            Type.EmptyTypes)
    ];

    public static bool Prefix(Reflections __instance, ref Task __result)
    {
        if (__instance.Owner is null
            || !PermanentDowngradeEventPatchSupport.IsAvailable)
        {
            return true;
        }

        __result = TouchMirror(__instance);
        return false;
    }

    private static async Task TouchMirror(Reflections reflections)
    {
        var owner = reflections.Owner!;
        var upgradedCards = owner.Deck.Cards
            .Where(BenMingGuRankProtection.CanBePermanentlyDowngraded)
            .ToList();

        for (var i = 0; i < 2 && upgradedCards.Count > 0; i++)
        {
            var card = reflections.Rng.NextItem(upgradedCards)!;
            upgradedCards.Remove(card);
            CardCmd.Downgrade(card);
            CardCmd.Preview(card, 1.2f, CardPreviewStyle.MessyLayout);
            await Cmd.CustomScaledWait(0.3f, 0.5f);
        }

        var upgradableCards = owner.Deck.Cards
            .Where(static card => card.IsUpgradable)
            .ToList();

        for (var i = 0; i < 4 && upgradableCards.Count > 0; i++)
        {
            var card = reflections.Rng.NextItem(upgradableCards)!;
            upgradableCards.Remove(card);
            CardCmd.Upgrade(card, CardPreviewStyle.MessyLayout);
            await Cmd.CustomScaledWait(0.3f, 0.5f);
        }

        await Cmd.CustomScaledWait(0.6f, 1.2f);
        PermanentDowngradeEventPatchSupport.Finish(
            reflections,
            "REFLECTIONS.pages.TOUCH_A_MIRROR.description");
    }
}

public sealed class WongoBenMingGuPatch : IPatchMethod
{
    public static string PatchId => "wongo-ben-ming-gu-rank-floor";

    public static string Description =>
        "Applies the aperture rank floor to BenMingGu downgrades when leaving Wongo's.";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(
            typeof(WelcomeToWongos),
            "Leave",
            Type.EmptyTypes)
    ];

    public static bool Prefix(WelcomeToWongos __instance, ref Task __result)
    {
        if (__instance.Owner is null
            || !PermanentDowngradeEventPatchSupport.IsAvailable)
        {
            return true;
        }

        __result = Leave(__instance);
        return false;
    }

    private static async Task Leave(WelcomeToWongos wongo)
    {
        var card = wongo.Rng.NextItem(
            wongo.Owner!.Deck.Cards.Where(
                BenMingGuRankProtection.CanBePermanentlyDowngraded));

        if (card is not null)
        {
            CardCmd.Downgrade(card);
            CardCmd.Preview(card);
            await Cmd.CustomScaledWait(0.5f, 1.2f);
        }

        PermanentDowngradeEventPatchSupport.Finish(
            wongo,
            "WELCOME_TO_WONGOS.pages.LEAVE.description");
    }
}
