using GuZhenRen.Powers;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Patching.Models;

namespace GuZhenRen.Patches;

public sealed class HuaShaClearBlockPatch : IPatchMethod
{
    public static string PatchId => "hua_sha_clear_block";

    public static string Description => "Hua Sha reacts when block is cleared";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(typeof(Creature), "ClearBlock", Type.EmptyTypes)
    ];

    public static void Prefix(Creature __instance, out int __state)
    {
        __state = __instance.Block;
    }

    public static void Postfix(
        Creature __instance,
        int __state,
        ref Task __result)
    {
        __result = TriggerAfterOriginal(__result, __instance, __state);
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
