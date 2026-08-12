using System.Runtime.CompilerServices;
using GuZhenRen.Cards;
using GuZhenRen.Characters;
using GuZhenRen.Relics;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using STS2RitsuLib.Patching.Models;

namespace GuZhenRen.Patches;

internal sealed class OrobasTransferData
{
    public required int Rank { get; init; }
    public required int Xp { get; init; }
    public required int MaxHpBonusApplied { get; init; }
}

internal static class OrobasTransferState
{
    public static readonly ConditionalWeakTable<Player, OrobasTransferData> Pending = [];
}

public sealed class OrobasFangYuanSetupPatch : IPatchMethod
{
    public static string PatchId => "orobas-fang-yuan-setup";

    public static string Description =>
        "Set up Touch of Orobas for Fang Yuan's current aperture.";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(
            typeof(TouchOfOrobas),
            nameof(TouchOfOrobas.SetupForPlayer),
            [typeof(Player)])
    ];

    public static bool Prefix(
        TouchOfOrobas __instance,
        Player player,
        ref bool __result)
    {
        if (player.Character is not FangYuanCharacter)
        {
            return true;
        }

        var aperture = player.Relics
            .OfType<AbstractKongQiaoRelic>()
            .FirstOrDefault();
        if (aperture is null)
        {
            __result = false;
            return false;
        }

        __instance.StarterRelic = aperture.Id;
        __instance.UpgradedRelic = ModelDb.Relic<XianTaiGu>().Id;
        __result = true;
        return false;
    }
}

public sealed class OrobasFangYuanObtainPatch : IPatchMethod
{
    public static string PatchId => "orobas-fang-yuan-obtain";

    public static string Description =>
        "Capture Fang Yuan's aperture cultivation when Touch of Orobas is obtained.";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(
            typeof(TouchOfOrobas),
            nameof(TouchOfOrobas.AfterObtained),
            [])
    ];

    public static void Prefix(TouchOfOrobas __instance)
    {
        var player = __instance.Owner;
        if (player.Character is not FangYuanCharacter)
        {
            return;
        }

        var aperture = player.Relics
            .OfType<AbstractKongQiaoRelic>()
            .FirstOrDefault();
        if (aperture is null)
        {
            return;
        }

        OrobasTransferState.Pending.Remove(player);
        OrobasTransferState.Pending.Add(player, new OrobasTransferData
        {
            Rank = aperture.Rank,
            Xp = aperture.Xp,
            MaxHpBonusApplied = aperture.Rank >= 6
                ? Math.Max(aperture.Rank, aperture.MaxHpBonusApplied)
                : aperture.MaxHpBonusApplied
        });
    }
}

public sealed class OrobasFangYuanUpgradePatch : IPatchMethod
{
    public static string PatchId => "orobas-fang-yuan-upgrade";

    public static string Description =>
        "Upgrade any Fang Yuan aperture into Supreme Immortal Fetus Gu.";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(
            typeof(TouchOfOrobas),
            nameof(TouchOfOrobas.GetUpgradedStarterRelic),
            [typeof(RelicModel)])
    ];

    public static void Postfix(RelicModel starterRelic, ref RelicModel __result)
    {
        if (starterRelic is AbstractKongQiaoRelic)
        {
            __result = ModelDb.Relic<XianTaiGu>();
        }
    }
}

public sealed class XianTaiGuTransferPatch : IPatchMethod
{
    public static string PatchId => "xian-tai-gu-transfer";

    public static string Description =>
        "Transfer aperture cultivation when Touch of Orobas grants Supreme Immortal Fetus Gu.";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(
            typeof(XianTaiGu),
            nameof(XianTaiGu.AfterObtained),
            [])
    ];

    public static void Prefix(XianTaiGu __instance)
    {
        var owner = __instance.Owner;
        if (owner is null
            || !OrobasTransferState.Pending.TryGetValue(owner, out var transfer))
        {
            return;
        }

        __instance.CurrentRank = transfer.Rank;
        __instance.Xp = transfer.Xp;
        __instance.MaxHpBonusApplied = transfer.MaxHpBonusApplied;
        OrobasTransferState.Pending.Remove(owner);
    }
}

public sealed class ArchaicToothFangYuanSetupPatch : IPatchMethod
{
    public static string PatchId => "archaic-tooth-fang-yuan-setup";

    public static string Description =>
        "Initialize Archaic Tooth when Fang Yuan obtains it through a direct grant.";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(
            typeof(ArchaicTooth),
            nameof(ArchaicTooth.AfterObtained),
            [])
    ];

    public static void Prefix(ArchaicTooth __instance)
    {
        var owner = __instance.Owner;
        if (owner.Character is not FangYuanCharacter
            || __instance.StarterCard is not null
            || !owner.Deck.Cards.Any(card => card is XiaoGuangGu))
        {
            return;
        }

        if (__instance.SetupForPlayer(owner))
        {
            Entry.Logger.Info(
                "Initialized Archaic Tooth for Fang Yuan through its direct-obtain path.");
        }
    }
}
