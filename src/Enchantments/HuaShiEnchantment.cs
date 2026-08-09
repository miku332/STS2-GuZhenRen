using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Enchantments;

[RegisterEnchantment]
public sealed class HuaShiEnchantment : ModEnchantmentTemplate
{
    public override bool IsStackable => true;

    public override bool ShowAmount => true;

    public override bool HasExtraCardText => true;

    public override EnchantmentAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/enchantments/HuaShiEnchantment.png");

    public override bool CanEnchant(CardModel card)
    {
        return base.CanEnchant(card)
            && (card.Enchantment is null
                || card.Enchantment is HuaShiEnchantment);
    }

    public override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay? cardPlay)
    {
        if (cardPlay is null || Card.Owner?.Creature is null || Amount <= 0)
        {
            return;
        }

        await CreatureCmd.GainBlock(
            Card.Owner.Creature,
            Amount,
            ValueProp.Move,
            cardPlay,
            false);
    }
}
