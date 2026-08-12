using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class XianAttackPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.None;

    protected override bool IsVisibleInternal => false;

    public override bool ShouldPlayVfx => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/LuDaoDaoHenPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/LuDaoDaoHenPower_p.png");

    public override async Task AfterAttack(
        PlayerChoiceContext choiceContext,
        AttackCommand command)
    {
        if (command.Attacker != Owner
            || command.ModelSource is not Cards.Xian)
        {
            return;
        }

        Entry.Logger.Info("[Xian] AfterAttack received.");

        var results = command.Results
            .SelectMany(static hit => hit)
            .GroupBy(result => result.Receiver);

        foreach (var targetResults in results)
        {
            var unblockedDamage = targetResults.Sum(result => result.UnblockedDamage);
            Entry.Logger.Info(
                $"[Xian] Target={targetResults.Key}, " +
                $"UnblockedDamage={unblockedDamage}, " +
                $"StrengthBefore={targetResults.Key.GetPowerAmount<StrengthPower>()}.");

            if (unblockedDamage <= 0)
            {
                continue;
            }

            var strengthBefore = targetResults.Key.GetPowerAmount<StrengthPower>();
            await PowerCmd.Apply<StrengthPower>(
                choiceContext,
                targetResults.Key,
                -unblockedDamage,
                Owner,
                command.ModelSource as CardModel);
            var strengthAfter = targetResults.Key.GetPowerAmount<StrengthPower>();
            var actualLoss = Math.Max(0, strengthBefore - strengthAfter);
            if (actualLoss <= 0)
            {
                continue;
            }

            Entry.Logger.Info(
                $"[Xian] AfterAttack UnblockedDamage={unblockedDamage}, " +
                $"StrengthLoss={actualLoss}, StrengthAfter={strengthAfter}.");

            await PowerCmd.Apply<XianStrengthDownPower>(
                choiceContext,
                targetResults.Key,
                actualLoss,
                Owner,
                command.ModelSource as CardModel);
        }

        await PowerCmd.Remove(this);
    }
}
