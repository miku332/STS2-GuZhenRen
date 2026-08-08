using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class ZhuiMingHuoPower : ModPowerTemplate
{
    private const int BurnPerStack = 5;

    private static bool s_isSpreading;

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

    public static async void AfterAttackEnded(AttackEndedEvent evt)
    {
        try
        {
            await SpreadAfterEnemyAttack(evt);
        }
        catch (Exception ex)
        {
            Entry.Logger.Error($"[ZhuiMingHuo] Failed to spread after attack: {ex}");
        }
    }

    private static async Task SpreadAfterEnemyAttack(AttackEndedEvent evt)
    {
        if (s_isSpreading)
        {
            return;
        }

        var attacker = evt.Attack.Attacker;
        if (attacker is null
            || !attacker.IsAlive
            || attacker.Player is not null
            || (!attacker.IsPrimaryEnemy && !attacker.IsSecondaryEnemy))
        {
            return;
        }

        var combatState = evt.CombatState;
        if (combatState.HittableEnemies.All(enemy =>
                !enemy.IsAlive || enemy.GetPower<ZhuiMingHuoPower>() is null))
        {
            return;
        }

        s_isSpreading = true;
        try
        {
            var choiceContext = evt.ChoiceContext ?? new ThrowingPlayerChoiceContext();
            await PowerCmd.Apply<ZhuiMingHuoPower>(
                choiceContext,
                attacker,
                1,
                attacker,
                null);
        }
        finally
        {
            s_isSpreading = false;
        }
    }
}
