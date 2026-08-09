using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class XueDianXingBanPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/XueDianXingBanPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/XueDianXingBanPower_p.png");

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        return target == Owner
            && dealer?.GetPower<XueYuanMarkPower>() is { Amount: > 0 }
            && props.IsPoweredAttack()
            ? 0.5m
            : 1m;
    }

    public override async Task BeforeSideTurnStart(
        MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side == CombatSide.Player && participants.Contains(Owner))
        {
            await MegaCrit.Sts2.Core.Commands.PowerCmd.Remove(this);
        }
    }
}
