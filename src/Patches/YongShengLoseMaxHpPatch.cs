using GuZhenRen.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Patching.Models;

namespace GuZhenRen.Patches;

public sealed class YongShengLoseMaxHpPatch : IPatchMethod
{
    public static string PatchId => "yong-sheng-prevent-max-hp-loss";

    public static string Description =>
        "Yong Sheng prevents its owner from losing maximum health.";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(
            typeof(CreatureCmd),
            nameof(CreatureCmd.LoseMaxHp),
            [
                typeof(PlayerChoiceContext),
                typeof(Creature),
                typeof(decimal),
                typeof(bool)
            ])
    ];

    public static bool Prefix(Creature creature, ref Task __result)
    {
        var relic = creature.Player?.GetRelic<XianQiao10>();
        if (relic is null)
        {
            return true;
        }

        relic.Flash();
        __result = Task.CompletedTask;
        return false;
    }
}
