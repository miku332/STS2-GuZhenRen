using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class HongLeiGuPower : ModPowerTemplate
{
    private const int ResetTurns = 3;
    private const int Damage = 25;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/HongLeiGuPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/HongLeiGuPower_p.png");

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
        await CreatureCmd.Damage(
            new ThrowingPlayerChoiceContext(),
            target,
            Damage,
            ValueProp.Unpowered,
            Owner,
            null,
            null);

        if (Owner.IsAlive)
        {
            await PowerCmd.ModifyAmount(
                new ThrowingPlayerChoiceContext(),
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
