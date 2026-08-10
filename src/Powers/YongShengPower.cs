using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class YongShengPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.None;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/YongShengPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/YongShengPower_p.png");

    public override decimal ModifyHpLostAfterOstyLate(
        Creature target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner || amount <= 0)
        {
            return amount;
        }

        Flash();
        return 0;
    }

    public override bool TryModifyEnergyCostInCombat(
        CardModel card,
        decimal originalCost,
        out decimal modifiedCost)
    {
        modifiedCost = originalCost;
        if (card.Owner?.Creature != Owner)
        {
            return false;
        }

        modifiedCost = 0;
        return true;
    }

    public override Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        return player.Creature == Owner
            ? FillHand(choiceContext, player)
            : Task.CompletedTask;
    }

    public override Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        var player = cardPlay.Card.Owner;
        return player is not null && player.Creature == Owner
            ? FillHand(choiceContext, player)
            : Task.CompletedTask;
    }

    private static async Task FillHand(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        var hand = PileType.Hand.GetPile(player);
        var missing = CardPile.MaxCardsInHand - hand.Cards.Count;
        if (missing > 0)
        {
            await CardPileCmd.Draw(choiceContext, missing, player);
        }
    }
}
