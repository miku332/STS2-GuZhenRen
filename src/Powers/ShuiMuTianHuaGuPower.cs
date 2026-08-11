using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class ShuiMuTianHuaGuPower : ModPowerTemplate
{
    private bool _lostHpSinceLastEnemyTurn;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/ShuiMuTianHuaGuPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/ShuiMuTianHuaGuPower_p.png");

    public override Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target == Owner && result.UnblockedDamage > 0)
        {
            _lostHpSinceLastEnemyTurn = true;
        }

        return Task.CompletedTask;
    }

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

        if (!_lostHpSinceLastEnemyTurn && Amount > 0)
        {
            Flash();
            await CreatureCmd.GainBlock(
                Owner,
                Amount,
                ValueProp.Unpowered,
                null);
        }

        _lostHpSinceLastEnemyTurn = false;
    }
}
