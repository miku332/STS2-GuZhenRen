using System.Collections.Generic;
using System.Threading.Tasks;
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
public sealed class GuCiPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/GuCiPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/GuCiPower_p.png");

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner
            || dealer is null
            || dealer == Owner
            || dealer.Player is not null
            || Amount <= 0
            || !props.IsPoweredAttack())
        {
            return;
        }

        var blockedDamage = result.TotalDamage - result.UnblockedDamage;
        if (blockedDamage <= 0)
        {
            return;
        }

        Flash();
        await CreatureCmd.Damage(
            choiceContext,
            dealer,
            blockedDamage,
            ValueProp.Unpowered,
            Owner,
            null,
            null);
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Enemy)
        {
            if (Amount <= 1)
            {
                await PowerCmd.Remove(this);
            }
            else
            {
                await PowerCmd.Decrement(this);
            }
        }
    }
}
