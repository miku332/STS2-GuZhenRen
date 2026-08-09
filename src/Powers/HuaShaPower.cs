using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class HuaShaPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.None;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/HuaShaPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/HuaShaPower_p.png");

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner)
        {
            return;
        }

        var lostBlock = result.TotalDamage - result.UnblockedDamage;
        await Trigger(choiceContext, lostBlock);
    }

    public async Task Trigger(
        PlayerChoiceContext choiceContext,
        decimal lostBlock)
    {
        if (lostBlock <= 0 || Owner.CombatState is null || !Owner.IsAlive)
        {
            return;
        }

        var enemies = Owner.CombatState.HittableEnemies.ToList();
        if (enemies.Count == 0)
        {
            return;
        }

        Flash();
        foreach (var enemy in enemies)
        {
            if (!enemy.IsAlive)
            {
                continue;
            }

            await CreatureCmd.Damage(
                choiceContext,
                enemy,
                lostBlock,
                ValueProp.Unpowered,
                Owner,
                null);
        }
    }
}
