using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class YingShengChongPower : ModPowerTemplate
{
    private string _targetCardName = string.Empty;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.None;

    public override LocString Description
    {
        get
        {
            var description = base.Description;
            description.Add("CardName", _targetCardName);
            description.Add("HasTarget", !string.IsNullOrEmpty(_targetCardName));
            return description;
        }
    }

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/YingShengChongPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/YingShengChongPower_p.png");

    public override Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (!Owner.IsAlive || player.Creature != GetAffectedPlayer())
        {
            return Task.CompletedTask;
        }

        var cards = PileType.Hand.GetPile(player).Cards;
        if (cards.Count == 0)
        {
            SetTarget(string.Empty);
            return Task.CompletedTask;
        }

        var target = player.RunState.Rng.CombatCardSelection.NextItem(cards)!;
        SetTarget(target.Title);
        Flash();
        Entry.Logger.Info(
            $"[Tribulation:YingShengChong] Locked {target.Id.Entry}.");
        return Task.CompletedTask;
    }

    public override async Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (!Owner.IsAlive
            || cardPlay.Card.Owner.Creature != GetAffectedPlayer()
            || string.IsNullOrEmpty(_targetCardName)
            || !string.Equals(
                cardPlay.Card.Title,
                _targetCardName,
                StringComparison.Ordinal))
        {
            return;
        }

        Flash();
        Entry.Logger.Info(
            $"[Tribulation:YingShengChong] Killed player for playing {cardPlay.Card.Id.Entry}.");
        await CreatureCmd.Kill(cardPlay.Card.Owner.Creature);
    }

    private void SetTarget(string cardName)
    {
        _targetCardName = cardName;
        InvokeDisplayAmountChanged();
    }

    private Creature? GetAffectedPlayer() =>
        Applier?.Player is not null
            ? Applier
            : Owner.CombatState?.Players.FirstOrDefault()?.Creature;
}
