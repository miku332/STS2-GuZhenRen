using GuZhenRen.Cards;
using GuZhenRen.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class XianQiaoBengKuiPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override LocString Description
    {
        get
        {
            var description = new LocString(
                "powers",
                "GU_ZHEN_REN_POWER_XIAN_QIAO_BENG_KUI_POWER.description");
            description.Add("Amount", Amount);
            return description;
        }
    }

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/XianQiaoBengKuiPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/XianQiaoBengKuiPower_p.png");

    public override decimal ModifyHandDraw(Player player, decimal count) =>
        player.Creature == Owner ? Math.Max(0, count - 2) : count;

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (player.Creature != Owner || Amount <= 0)
        {
            return;
        }

        Flash();
        await PlayerCmd.LoseEnergy(2, player);
        await CreatureCmd.Damage(
            choiceContext,
            Owner,
            10,
            ValueProp.Unblockable | ValueProp.Unpowered,
            Applier,
            null,
            null);

        if (Amount > 1)
        {
            await PowerCmd.Decrement(this);
            return;
        }

        await DestroyBenMingGuAndAperture(player);
        if (Owner.IsAlive)
        {
            await PowerCmd.Remove(this);
        }
    }

    private static async Task DestroyBenMingGuAndAperture(Player player)
    {
        AbstractBenMingGuCard.IsSynthesizing = true;
        try
        {
            var deckCards = player.Deck.Cards
                .OfType<AbstractBenMingGuCard>()
                .Cast<CardModel>()
                .ToList();
            if (deckCards.Count > 0)
            {
                await CardPileCmd.RemoveFromDeck(deckCards, showPreview: true);
            }

            var combatCards = player.PlayerCombatState?.AllCards
                .OfType<AbstractBenMingGuCard>()
                .Cast<CardModel>()
                .ToList() ?? [];
            if (combatCards.Count > 0)
            {
                await CardPileCmd.RemoveFromCombat(combatCards);
            }
        }
        finally
        {
            AbstractBenMingGuCard.IsSynthesizing = false;
        }

        foreach (var aperture in player.Relics
                     .OfType<AbstractKongQiaoRelic>()
                     .Where(relic => relic.Rank <= 9)
                     .ToList())
        {
            await RelicCmd.Remove(aperture);
        }
    }
}
