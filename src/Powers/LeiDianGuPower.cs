using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class LeiDianGuPower : ModPowerTemplate
{
    private const int ResetTurns = 3;
    private const int HitCount = 5;
    private const int DamagePerHit = 8;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/LeiDianGuPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/LeiDianGuPower_p.png");

    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side != CombatSide.Enemy
            || !Owner.IsAlive
            || !participants.Contains(Owner))
        {
            return;
        }

        if (Amount > 1)
        {
            Flash();
            await PowerCmd.Decrement(this);
            return;
        }

        var target = GetPlayerTarget(combatState);
        if (target is null)
        {
            return;
        }

        Flash();
        var choiceContext = new ThrowingPlayerChoiceContext();
        for (var i = 0; i < HitCount && target.IsAlive; i++)
        {
            var results = await CreatureCmd.Damage(
                choiceContext,
                target,
                DamagePerHit,
                ValueProp.Unpowered,
                Owner,
                null);
            if (results.Any(result =>
                    result.Receiver == target && result.UnblockedDamage > 0))
            {
                await CardPileCmd.AddToCombatAndPreview<Dazed>(
                    target,
                    PileType.Draw,
                    1,
                    null,
                    CardPilePosition.Top);
            }
        }

        if (Owner.IsAlive)
        {
            await PowerCmd.ModifyAmount(
                choiceContext,
                this,
                ResetTurns - Amount,
                Owner,
                null);
        }
    }

    private Creature? GetPlayerTarget(ICombatState combatState) =>
        Applier?.Player is not null && Applier.IsAlive
            ? Applier
            : combatState.Players.FirstOrDefault(player =>
                player.Creature.IsAlive)?.Creature;
}
