using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class XiaoJiaZiQiPower : ModPowerTemplate
{
    private HashSet<CardTag> _lastDaoTags = [];

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/XiaoJiaZiQiPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/XiaoJiaZiQiPower_p.png");

    public override Task AfterApplied(
        Creature? applier,
        CardModel? cardSource)
    {
        // The application amount only keeps the power alive; accumulated
        // damage starts at zero and is stored in Amount for native UI refresh.
        SetAmount(0);
        return Task.CompletedTask;
    }

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (!Owner.IsAlive || cardPlay.Card.Owner.Creature == Owner)
        {
            return;
        }

        var currentTags = GuZhenRenTagRules
            .GetEffectiveDaoTags(cardPlay.Card)
            .ToHashSet();
        if (currentTags.Count > 0
            && cardPlay.Card.Owner.Creature.GetPower<RuiYiPower>() is not null)
        {
            currentTags = [GuZhenRenTags.JianDao];
        }

        if (currentTags.Count > 0
            && _lastDaoTags.Overlaps(currentTags))
        {
            Flash();
            await PowerCmd.ModifyAmount(
                choiceContext,
                this,
                5,
                Owner,
                cardPlay.Card);
            Entry.Logger.Info(
                $"[Tribulation:XiaoJiaZiQi] Pending damage increased to {Amount}.");
        }

        _lastDaoTags = currentTags;
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player || !Owner.IsAlive)
        {
            return;
        }

        var target = Applier?.Player is not null && Applier.IsAlive
            ? Applier
            : Owner.CombatState?.Players.FirstOrDefault(player =>
                player.Creature.IsAlive)?.Creature;
        var pendingDamage = Amount;
        if (pendingDamage > 0 && target is not null)
        {
            Flash();
            await CreatureCmd.Damage(
                choiceContext,
                target,
                pendingDamage,
                ValueProp.Unpowered,
                Owner,
                null,
                null);
        }

        // SetAmount avoids removing this persistent power at zero.
        SetAmount(0);
        _lastDaoTags.Clear();
    }
}
