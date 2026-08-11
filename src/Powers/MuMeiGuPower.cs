using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class MuMeiGuPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.None;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/MuMeiGuPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/MuMeiGuPower_p.png");

    public override async Task AfterApplied(
        Creature? applier,
        CardModel? cardSource)
    {
        var maxHpLoss = Math.Floor(Owner.MaxHp / 2m);
        if (maxHpLoss <= 0)
        {
            return;
        }

        await CreatureCmd.LoseMaxHp(
            new ThrowingPlayerChoiceContext(),
            Owner,
            maxHpLoss,
            false);
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Enemy
            || !Owner.IsAlive
            || !participants.Contains(Owner))
        {
            return;
        }

        var missingHp = Math.Max(0m, Owner.MaxHp - Owner.CurrentHp);
        var healing = Math.Ceiling(missingHp * 0.95m);
        if (healing <= 0)
        {
            return;
        }

        Flash();
        await CreatureCmd.Heal(Owner, healing);
    }
}
