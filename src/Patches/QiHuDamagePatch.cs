using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Patching.Models;

namespace GuZhenRen.Patches;

public sealed class QiHuDamagePatch : IPatchMethod
{
    public static string PatchId => "qi_hu_damage_redirection";

    public static string Description =>
        "Qi Hu redirects damage from Long Gong to the Qi Wall";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(
            typeof(CreatureCmd),
            nameof(CreatureCmd.Damage),
            [
                typeof(PlayerChoiceContext),
                typeof(IEnumerable<Creature>),
                typeof(decimal),
                typeof(ValueProp),
                typeof(Creature),
                typeof(CardModel),
                typeof(CardPlay)
            ])
    ];

    public static void Prefix(ref IEnumerable<Creature> targets)
    {
        var originalTargets = targets.ToList();
        var redirectedTargets = new List<Creature>(originalTargets.Count);
        var changed = false;

        foreach (var target in originalTargets)
        {
            if (QiHuState.TryRedirectDamage(target, out var redirectedTarget))
            {
                changed = true;
            }

            redirectedTargets.Add(redirectedTarget);
        }

        if (changed)
        {
            targets = redirectedTargets;
        }
    }
}
