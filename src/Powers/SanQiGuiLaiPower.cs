using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class SanQiGuiLaiPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.None;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/SanQiGuiLaiPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/SanQiGuiLaiPower_p.png");

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Enemy
            && Owner.IsAlive
            && participants.Contains(Owner)
            && Owner.CurrentHp < Owner.MaxHp)
        {
            Flash();
            await CreatureCmd.Heal(Owner, Owner.MaxHp - Owner.CurrentHp);
        }
    }
}
