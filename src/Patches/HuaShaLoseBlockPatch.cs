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
            [
                typeof(PlayerChoiceContext),
                typeof(Creature),
                typeof(decimal),
                typeof(Creature)
            ])
    ];

    public static void Prefix(Creature target, out int __state)
    {
        __state = target.Block;
    }

    public static void Postfix(
        PlayerChoiceContext choiceContext,
        Creature target,
        int __state,
        ref Task __result)
    {
        __result = TriggerAfterOriginal(
            __result,
            choiceContext,
            target,
            __state);
    }

    private static async Task TriggerAfterOriginal(
        Task original,
        PlayerChoiceContext choiceContext,
        Creature creature,
        int blockBefore)
    {
        await original;

        var lostBlock = blockBefore - creature.Block;
        var power = creature.GetPower<HuaShaPower>();
        if (lostBlock > 0 && power is not null)
        {
            await power.Trigger(choiceContext, lostBlock);
        }
    }
}
