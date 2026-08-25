using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class ZhuiMingHuoPower : ModPowerTemplate
{
    private const int BurnPerStack = 5;

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/ZhuiMingHuoPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/ZhuiMingHuoPower_p.png");

    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (Amount <= 0 || !Owner.IsAlive || !participants.Contains(Owner))
        {
            return;
        }

        Flash();
        await PowerCmd.Apply<FenShaoPower>(
            new ThrowingPlayerChoiceContext(),
            Owner,
            BurnPerStack * Amount,
            Owner,
            null);
    }

    public override async Task AfterAttack(
        PlayerChoiceContext choiceContext,
        AttackCommand command)
    {
        var attacker = command.Attacker;
        if (attacker is null
            || !attacker.IsAlive
            || attacker.Player is not null
            || (!attacker.IsPrimaryEnemy && !attacker.IsSecondaryEnemy))
        {
            return;
        }

        var combatState = Owner.CombatState;
        if (combatState is null)
        {
            return;
        }

        var coordinator = combatState.HittableEnemies
            .Where(static enemy => enemy.IsAlive)
            .Select(static enemy => enemy.GetPower<ZhuiMingHuoPower>())
            .FirstOrDefault(static power => power is not null);
        if (coordinator != this)
        {
            return;
        }

        await PowerCmd.Apply<ZhuiMingHuoPower>(
            choiceContext,
            attacker,
            1,
            attacker,
            null);
    }
}
