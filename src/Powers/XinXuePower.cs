using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class XinXuePower : ModPowerTemplate
{
    private bool _isPlayerTurn;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/XinXuePower.png",
        BigIconPath: "res://GuZhenRen/images/powers/XinXuePower_p.png");

    public override Task AfterApplied(
        Creature? applier,
        CardModel? cardSource)
    {
        _isPlayerTurn = Owner.Player is not null;
        return Task.CompletedTask;
    }

    public override Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (player == Owner.Player)
        {
            _isPlayerTurn = true;
        }

        return Task.CompletedTask;
    }

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner
            || Owner.Player is null
            || !_isPlayerTurn
            || result.UnblockedDamage <= 0
            || Amount <= 0)
        {
            return;
        }

        var combatState = Owner.CombatState;
        if (combatState is null)
        {
            return;
        }

        Flash();
        var hpLoss = result.UnblockedDamage * Amount;
        foreach (var enemy in combatState.HittableEnemies.ToList())
        {
            await CreatureCmd.Damage(
                choiceContext,
                enemy,
                hpLoss,
                ValueProp.Unblockable | ValueProp.Unpowered,
                Owner,
                null,
                null);
        }
    }

    public override Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Player)
        {
            _isPlayerTurn = false;
        }

        return Task.CompletedTask;
    }
}
