using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

namespace GuZhenRen.Systems;

internal sealed class RecipeIngredient(
    string promptKey,
    Func<CardModel, bool> matches)
{
    public LocString Prompt => new(
        "card_selection",
        $"GU_ZHEN_REN_{promptKey}");

    public bool Matches(CardModel card) => matches(card);
}
