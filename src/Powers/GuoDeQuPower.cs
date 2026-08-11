using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class GuoDeQuPower : ModPowerTemplate
{
    private CardModel? _nullifiedCard;
    private bool _triggeredThisTurn;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.None;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/GuoDeQuPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/GuoDeQuPower_p.png");

    public override Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (player.Creature == GetAffectedPlayer())
        {
            _triggeredThisTurn = false;
            _nullifiedCard = null;
        }

        return Task.CompletedTask;
    }

    public override int ModifyCardPlayCount(
        CardModel card,
        Creature? target,
        int playCount)
    {
        if (_triggeredThisTurn
            || !Owner.IsAlive
            || card.Owner.Creature != GetAffectedPlayer())
        {
            return playCount;
        }

        _triggeredThisTurn = true;
        _nullifiedCard = card;
        return 0;
    }

    public override Task AfterModifyingCardPlayCount(CardModel card)
    {
        if (ReferenceEquals(_nullifiedCard, card))
        {
            _nullifiedCard = null;
            Flash();
            Entry.Logger.Info(
                $"[Tribulation:GuoDeQu] Nullified {card.Id.Entry}.");
        }

        return Task.CompletedTask;
    }

    private Creature? GetAffectedPlayer() =>
        Applier?.Player is not null
            ? Applier
            : Owner.CombatState?.Players.FirstOrDefault()?.Creature;
}
