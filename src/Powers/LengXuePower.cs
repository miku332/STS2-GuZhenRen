using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
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
public sealed class LengXuePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/LengXuePower.png",
        BigIconPath: "res://GuZhenRen/images/powers/LengXuePower_p.png");

    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (Amount <= 0 || !Owner.IsAlive || !participants.Contains(Owner))
        {
            return;
        }

        Flash();

        var damage = Math.Max(1m, Math.Floor(Owner.MaxHp / 10m));
        await CreatureCmd.Damage(
            new ThrowingPlayerChoiceContext(),
            Owner,
            damage,
            ValueProp.Unblockable | ValueProp.Unpowered,
            Owner,
            null,
            null);

        if (Owner.IsAlive)
        {
            await PowerCmd.Decrement(this);
        }
        else
        {
            await Cmd.CustomScaledWait(0.1f, 0.25f);
        }
    }
}
