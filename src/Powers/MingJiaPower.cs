using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class MingJiaPower : ModPowerTemplate
{
    private const int BufferAmount = 99;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.None;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/MingJiaPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/MingJiaPower_p.png");

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

        Flash();
        var choiceContext = new ThrowingPlayerChoiceContext();
        var player = Applier?.Player ?? combatState.Players.FirstOrDefault();
        if (player is null)
        {
            return;
        }

        if (player.RunState.Rng.CombatTargets.NextBool())
        {
            await PowerCmd.Apply<BufferPower>(
                choiceContext,
                Owner,
                BufferAmount,
                Owner,
                null);
            return;
        }

        var buffer = Owner.GetPower<BufferPower>();
        if (buffer is not null)
        {
            await PowerCmd.Remove(buffer);
        }
    }
}
