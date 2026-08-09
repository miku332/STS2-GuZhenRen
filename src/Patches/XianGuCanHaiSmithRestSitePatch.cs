using System.Runtime.CompilerServices;
using GuZhenRen.Relics;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using STS2RitsuLib.Patching.Models;

namespace GuZhenRen.Patches;

public sealed class XianGuCanHaiSmithRestSitePatch : IPatchMethod
{
    private sealed class ChoiceCache
    {
        public bool WasSmith { get; set; }

        public List<RestSiteOption>? OptionsBeforeChoice { get; set; }
    }

    private static readonly ConditionalWeakTable<Player, ChoiceCache> Cache = [];

    public static string PatchId => "xian-gu-can-hai-smith-rest-site";

    public static string Description => "Allows XianGuCanHai to grant additional smiths without consuming the rest site.";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(
            typeof(RestSiteSynchronizer),
            "ChooseOption",
            [typeof(Player), typeof(int)])
    ];

    public static void Prefix(
        RestSiteSynchronizer __instance,
        Player player,
        int optionIndex)
    {
        var options = __instance.GetOptionsForPlayer(player);
        var cache = Cache.GetOrCreateValue(player);
        cache.WasSmith = optionIndex >= 0
            && optionIndex < options.Count
            && options[optionIndex] is SmithRestSiteOption;
        cache.OptionsBeforeChoice = cache.WasSmith ? options.ToList() : null;
    }

    public static void Postfix(
        RestSiteSynchronizer __instance,
        Player player,
        ref Task<bool> __result)
    {
        __result = RestoreOptionsAfterSmith(
            __instance,
            player,
            __result);
    }

    private static async Task<bool> RestoreOptionsAfterSmith(
        RestSiteSynchronizer synchronizer,
        Player player,
        Task<bool> original)
    {
        var success = await original;
        if (!success)
        {
            return false;
        }

        if (!Cache.TryGetValue(player, out var cache)
            || !cache.WasSmith
            || cache.OptionsBeforeChoice is null)
        {
            return true;
        }

        var relic = player.GetRelic<XianGuCanHai>();
        if (relic is null || relic.Counter <= 0)
        {
            return true;
        }

        relic.Counter--;
        relic.Flash();

        if (synchronizer.GetOptionsForPlayer(player) is List<RestSiteOption> options)
        {
            options.Clear();
            options.AddRange(cache.OptionsBeforeChoice);
        }

        return true;
    }
}
