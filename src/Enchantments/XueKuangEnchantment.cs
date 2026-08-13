using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Enchantments;

[RegisterEnchantment]
public sealed class XueKuangEnchantment : ModEnchantmentTemplate
{
    public override bool HasExtraCardText => true;

    public override EnchantmentAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/enchantments/XueKuangEnchantment.svg");

    public override bool CanEnchant(CardModel card) =>
        base.CanEnchant(card)
        && (card.Enchantment is null || card.Enchantment is XueKuangEnchantment);

    public override Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay? cardPlay) =>
        Task.CompletedTask;
}
