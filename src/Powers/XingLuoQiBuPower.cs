using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class XingLuoQiBuPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/XingLuoQiBuPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/XingLuoQiBuPower_p.png");

    public static async Task TriggerBeforeNianGain(Creature owner, decimal nianGained)
    {
        var power = owner.GetPower<XingLuoQiBuPower>();
        if (power is null || power.Amount <= 0 || nianGained <= 0)
        {
            return;
        }

        power.Flash();
        await CreatureCmd.GainBlock(
            owner,
            power.Amount * nianGained,
            ValueProp.Unpowered,
            null,
            fast: true);
    }

    public override async Task BeforeSideTurnStart(
        MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side == CombatSide.Player && participants.Contains(Owner))
        {
            await PowerCmd.Remove(this);
        }
    }
}
