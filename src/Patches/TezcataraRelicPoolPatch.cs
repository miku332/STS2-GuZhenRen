using System.Threading.Tasks;
using GuZhenRen.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using STS2RitsuLib.Patching.Models;

namespace GuZhenRen.Patches;

public sealed class TezcataraRelicPoolPatch : IPatchMethod
{
    public static string PatchId => "tezcatara-relic-pool";

    public static string Description =>
        "Adds Fixed Immortal Travel to Tezcatara's relic pool.";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(
            typeof(Tezcatara),
            "get_OptionPool3",
            [])
    ];

    public static void Postfix(
        Tezcatara __instance,
        ref List<EventOption> __result)
    {
        var relic = ModelDb.Relic<DingXianYou>().ToMutable();
        var owner = __instance.Owner;
        if (owner is not null)
        {
            relic.Owner = owner;
        }

        __result.Add(EventOption.FromRelic(
            relic,
            __instance,
            async () =>
            {
                var currentOwner = __instance.Owner;
                if (currentOwner is null)
                {
                    return;
                }

                relic.Owner = currentOwner;
                await RelicCmd.Obtain(relic, currentOwner);
                __instance.StartPreFinished();
            },
            "TEZCATARA.pages.INITIAL.options.GU_ZHEN_REN_RELIC_DING_XIAN_YOU"));
    }
}
