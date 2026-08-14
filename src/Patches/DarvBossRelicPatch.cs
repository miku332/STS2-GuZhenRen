using System.Collections;
using System.Reflection;
using GuZhenRen.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using STS2RitsuLib.Patching.Models;

namespace GuZhenRen.Patches;

public sealed class DarvBossRelicPatch : IPatchMethod
{
    private static readonly object SyncRoot = new();

    public static string PatchId => "darv-boss-relic-pool";

    public static string Description =>
        "Adds Fixed Immortal Travel to Darv's boss relic pool.";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(
            typeof(Darv),
            "GenerateInitialOptions",
            []),
        new ModPatchTarget(
            typeof(Darv),
            "get_AllPossibleOptions",
            [])
    ];

    public static void Prefix()
    {
        lock (SyncRoot)
        {
            var validRelicSetType = typeof(Darv).GetNestedType(
                "ValidRelicSet",
                BindingFlags.NonPublic);
            var validRelicSetsField = typeof(Darv).GetField(
                "_validRelicSets",
                BindingFlags.NonPublic | BindingFlags.Static);
            var relicsField = validRelicSetType?.GetField(
                "relics",
                BindingFlags.Public | BindingFlags.Instance);

            if (validRelicSetType is null
                || validRelicSetsField?.GetValue(null) is not IList validRelicSets
                || relicsField is null)
            {
                Entry.Logger.Warn(
                    "Could not access Darv's boss relic pool; Fixed Immortal Travel was not added.");
                return;
            }

            foreach (var validRelicSet in validRelicSets)
            {
                if (validRelicSet is not null
                    && relicsField.GetValue(validRelicSet) is RelicModel[] relics
                    && relics.Any(relic => relic is DingXianYou))
                {
                    return;
                }
            }

            var newRelicSet = Activator.CreateInstance(
                validRelicSetType,
                [new RelicModel[] { ModelDb.Relic<DingXianYou>() }]);
            if (newRelicSet is null)
            {
                Entry.Logger.Warn(
                    "Could not create Darv's Fixed Immortal Travel relic pool entry.");
                return;
            }

            validRelicSets.Add(newRelicSet);
            Entry.Logger.Info(
                "Added Fixed Immortal Travel to Darv's boss relic pool.");
        }
    }
}
