using System.Threading.Tasks;
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
public sealed class XueYuanPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/XueYuanPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/XueYuanPower_p.png");

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
            || result.UnblockedDamage <= 0)
        {
            return;
        }

        var combatState = Owner.CombatState;
        if (combatState is null)
        {
            return;
        }

        var markedEnemies = combatState.HittableEnemies
            .Select(enemy => new
            {
                Enemy = enemy,
                Mark = enemy.GetPower<XueYuanMarkPower>()
            })
            .Where(entry => entry.Mark?.Amount > 0)
            .ToList();

        if (markedEnemies.Count == 0)
        {
            return;
        }

        Flash();
        foreach (var entry in markedEnemies)
        {
            await CreatureCmd.Damage(
                choiceContext,
                entry.Enemy,
                result.UnblockedDamage * entry.Mark!.Amount,
                ValueProp.Unblockable | ValueProp.Unpowered,
                Owner,
                null);
        }
    }
}
