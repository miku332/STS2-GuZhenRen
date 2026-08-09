using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class BuMieXingBiaoPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/BuMieXingBiaoPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/BuMieXingBiaoPower_p.png");

    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side != CombatSide.Enemy
            || Amount <= 0
            || !Owner.IsAlive
            || !participants.Contains(Owner))
        {
            return;
        }

        var player = combatState.Players.FirstOrDefault();
        if (player is null || !player.Creature.IsAlive)
        {
            return;
        }

        Flash();
        var choiceContext = new ThrowingPlayerChoiceContext();
        await PowerCmd.Apply<NianPower>(
            choiceContext,
            player.Creature,
            Amount,
            player.Creature,
            null);
        await PowerCmd.ModifyAmount(
            choiceContext,
            this,
            1,
            player.Creature,
            null);
    }
}
