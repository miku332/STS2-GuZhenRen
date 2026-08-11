using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class GuanPower : ModPowerTemplate
{
    private int _threshold = 1;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override LocString Description
    {
        get
        {
            var description = base.Description;
            description.Add("Threshold", _threshold);
            return description;
        }
    }

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/GuanPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/GuanPower_p.png");

    public override Task AfterApplied(
        Creature? applier,
        CardModel? cardSource)
    {
        _threshold = Math.Max(1, (int)(Owner.MaxHp * 0.2m));
        SetAmount(_threshold);
        return Task.CompletedTask;
    }

    public override Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner
            || !Owner.IsAlive
            || result.UnblockedDamage <= 0
            || Owner.CombatState?.CurrentSide != CombatSide.Player)
        {
            return Task.CompletedTask;
        }

        var remaining = Amount - result.UnblockedDamage;
        if (remaining > 0)
        {
            SetAmount(remaining);
            return Task.CompletedTask;
        }

        SetAmount(_threshold);
        Flash();
        var player = Applier?.Player
            ?? Owner.CombatState?.Players.FirstOrDefault();
        if (player is not null && player.Creature.IsAlive)
        {
            Entry.Logger.Info(
                $"[Tribulation:Guan] Forced turn end after {_threshold} HP lost.");
            PlayerCmd.EndTurn(player, canBackOut: false);
        }

        return Task.CompletedTask;
    }
}
