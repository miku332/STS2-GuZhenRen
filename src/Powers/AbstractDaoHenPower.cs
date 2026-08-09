using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

public abstract class AbstractDaoHenPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://GuZhenRen/images/powers/{GetType().Name}.png",
        BigIconPath: $"res://GuZhenRen/images/powers/{GetType().Name}_p.png");

    public virtual int GetDerivedPowerAmount(PowerModel power) => 0;

    public sealed override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (player.Creature != Owner || Amount <= 0)
        {
            return;
        }

        Flash();
        var amount = Amount;
        await BeforeResetToBianHua(choiceContext, Owner);
        await PowerCmd.Remove(this);
        await PowerCmd.Apply<BianHuaDaoDaoHenPower>(
            choiceContext,
            Owner,
            amount,
            Owner,
            null);
        await ZhuanYiPower.TriggerConversion(Owner, Owner, null);
    }

    protected virtual Task BeforeResetToBianHua(
        PlayerChoiceContext choiceContext,
        Creature owner) => Task.CompletedTask;
}
