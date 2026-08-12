using GuZhenRen.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Patching.Models;

namespace GuZhenRen.Patches;

public sealed class BenMingGuRemovalPenaltyPatch : IPatchMethod
{
    private const decimal RemovalDamageRatio = 0.80m;

    public static string PatchId => "ben-ming-gu-removal-penalty";

    public static string Description =>
        "Removing a BenMingGu from the deck costs 80% of maximum HP, matching the original mod.";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(
            typeof(CardPileCmd),
            nameof(CardPileCmd.RemoveFromDeck),
            [typeof(IReadOnlyList<CardModel>), typeof(bool)])
    ];

    public static void Postfix(
        IReadOnlyList<CardModel> cards,
        ref Task __result)
    {
        if (AbstractBenMingGuCard.IsSynthesizing
            || BenMingGuUniquenessPatch.IsRemovingDuplicate)
        {
            return;
        }

        var creatures = cards
            .OfType<AbstractBenMingGuCard>()
            .Select(card => card.Owner?.Creature)
            .Where(static creature => creature is not null)
            .Cast<Creature>()
            .ToList();

        if (creatures.Count == 0)
        {
            return;
        }

        __result = ApplyPenalties(__result, creatures);
    }

    private static async Task ApplyPenalties(
        Task original,
        IReadOnlyList<Creature> creatures)
    {
        await original;

        foreach (var creature in creatures)
        {
            if (!creature.IsAlive)
            {
                continue;
            }

            var damage = Math.Floor(creature.MaxHp * RemovalDamageRatio);
            damage = Math.Min(damage, Math.Max(0m, creature.CurrentHp - 1m));
            if (damage <= 0)
            {
                continue;
            }

            Entry.Logger.Info(
                $"BenMingGu removal penalty: {damage} HP loss for {creature.Player?.NetId}.");
            await CreatureCmd.Damage(
                new ThrowingPlayerChoiceContext(),
                creature,
                damage,
                ValueProp.Unblockable | ValueProp.Unpowered,
                null,
                null);
        }
    }
}
