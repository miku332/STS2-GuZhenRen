using GuZhenRen.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Patching.Models;

namespace GuZhenRen.Patches;

public sealed class HuaShaLoseBlockPatch : IPatchMethod
{
    public static string PatchId => "hua_sha_lose_block";

    public static string Description => "Hua Sha reacts to commanded block loss";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(
            typeof(CreatureCmd),
            nameof(CreatureCmd.LoseBlock),
            [typeof(Creature), typeof(decimal)])
    ];

    public static void Prefix(Creature creature, out int __state)
    {
        __state = creature.Block;
    }

    public static void Postfix(
        Creature creature,
        int __state,
        ref Task __result)
    {
        __result = TriggerAfterOriginal(__result, creature, __state);
    }

    private static async Task TriggerAfterOriginal(
        Task original,
        Creature creature,
        int blockBefore)
    {
        await original;

        var lostBlock = blockBefore - creature.Block;
        var power = creature.GetPower<HuaShaPower>();
        if (lostBlock > 0 && power is not null)
        {
            await power.Trigger(new ThrowingPlayerChoiceContext(), lostBlock);
        }
    }
}
