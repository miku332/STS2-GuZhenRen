using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class XianStrengthDownPower
    : ModPowerTemplate
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Enemy
            || !participants.Contains(Owner)
            || Amount <= 0)
        {
            return;
        }

        Flash();
        var amountToRestore = Amount;
        await PowerCmd.Apply<StrengthPower>(
            choiceContext,
            Owner,
            amountToRestore,
            Owner,
            null,
            silent: true);
        Entry.Logger.Info(
            $"[Xian] RestoredStrength={amountToRestore} at enemy turn end.");
        await PowerCmd.Remove(this);
    }

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/LuDaoDaoHenPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/LuDaoDaoHenPower_p.png");

    public override LocString Description => new(
        "powers",
        "GU_ZHEN_REN_POWER_XIAN_STRENGTH_DOWN_POWER.description");
}
