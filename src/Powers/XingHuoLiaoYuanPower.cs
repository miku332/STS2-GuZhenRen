using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class XingHuoLiaoYuanPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.None;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/XingHuoLiaoYuanPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/XingHuoLiaoYuanPower_p.png");

    public async Task Spread(
        PlayerChoiceContext choiceContext,
        decimal amountApplied,
        Creature? applier,
        CardModel? cardSource)
    {
        if (amountApplied <= 0 || Owner.CombatState is null || !Owner.IsAlive)
        {
            return;
        }

        Flash();
        foreach (var enemy in Owner.CombatState.HittableEnemies.ToList())
        {
            if (enemy == Owner || !enemy.IsAlive)
            {
                continue;
            }

            await PowerCmd.Apply<FenShaoPower>(
                choiceContext,
                enemy,
                amountApplied,
                applier,
                cardSource);
        }
    }
}
