using GuZhenRen.Cards;
using GuZhenRen.Relics;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;

namespace GuZhenRen.Systems;

internal static class ShaZhaoRecipeSystem
{
    public static IEnumerable<AbstractRecipeRelic> GetAvailableRecipes(
        Player player) =>
        player.Relics
            .OfType<AbstractRecipeRelic>()
            .Where(static recipe => !recipe.IsCrafted);

    public static IEnumerable<AbstractRecipeRelic> GetCraftableRecipes(
        Player player) =>
        GetAvailableRecipes(player).Where(recipe => CanCraft(player, recipe));

    public static async Task<bool> TryCraft(Player player)
    {
        var recipes = GetCraftableRecipes(player).ToList();
        if (recipes.Count == 0)
        {
            return false;
        }

        var recipe = await SelectRecipe(player, recipes);
        if (recipe is null)
        {
            return false;
        }

        var selectedIngredients = new List<CardModel>();
        foreach (var ingredient in recipe.Ingredients)
        {
            var selection = await CardSelectCmd.FromDeckGeneric(
                player,
                new CardSelectorPrefs(ingredient.Prompt, 1)
                {
                    Cancelable = true,
                    RequireManualConfirmation = true
                },
                card => !selectedIngredients.Contains(card)
                    && MatchesIngredient(ingredient, card));
            var selected = selection.FirstOrDefault();
            if (selected is null)
            {
                return false;
            }

            selectedIngredients.Add(selected);
        }

        if (!CanCraftWithSelection(recipe, selectedIngredients))
        {
            return false;
        }

        foreach (var ingredient in selectedIngredients)
        {
            await CardPileCmd.RemoveFromDeck(ingredient, showPreview: false);
        }

        var reward = player.RunState.CreateCard(recipe.RewardCard, player);
        reward.FloorAddedToDeck = player.RunState.TotalFloor;
        SaveManager.Instance.MarkCardAsSeen(reward);
        if (!player.DiscoveredCards.Contains(reward.Id))
        {
            player.DiscoveredCards.Add(reward.Id);
        }

        var result = await CardPileCmd.Add(
            reward,
            PileType.Deck,
            CardPilePosition.Bottom,
            recipe,
            false);
        if (!result.success)
        {
            Entry.Logger.Error(
                $"Failed to add crafted card {reward.Id.Entry} to the deck.");
            return false;
        }

        result.cardAdded.Pile?.InvokeCardAddFinished();
        recipe.IsCrafted = true;
        recipe.Flash();
        CardCmd.PreviewCardPileAdd([result], 2f);

        if (LocalContext.IsMe(player)
            && !RunManager.Instance.IsSingleplayerOrFakeMultiplayer)
        {
            RunManager.Instance.RewardSynchronizer.SyncLocalObtainedCard(
                reward);
        }

        return true;
    }

    private static async Task<AbstractRecipeRelic?> SelectRecipe(
        Player player,
        IReadOnlyList<AbstractRecipeRelic> recipes)
    {
        if (recipes.Count == 1)
        {
            return recipes[0];
        }

        var rewards = recipes.Select(recipe => recipe.RewardCard).ToList();
        var selected = (await CardSelectCmd.FromSimpleGrid(
            new BlockingPlayerChoiceContext(),
            rewards,
            player,
            new CardSelectorPrefs(
                new LocString("card_selection", "GU_ZHEN_REN_ASSEMBLE_CHOOSE_RECIPE"),
                1)
            {
                Cancelable = true,
                RequireManualConfirmation = true
            })).FirstOrDefault();

        return selected is null
            ? null
            : recipes.First(recipe => recipe.RewardCard.Id == selected.Id);
    }

    private static bool CanCraft(Player player, AbstractRecipeRelic recipe) =>
        CanMatchIngredients(
            recipe.Ingredients,
            player.Deck.Cards.ToList(),
            0);

    private static bool CanMatchIngredients(
        IReadOnlyList<RecipeIngredient> ingredients,
        List<CardModel> remainingCards,
        int ingredientIndex)
    {
        if (ingredientIndex >= ingredients.Count)
        {
            return true;
        }

        var ingredient = ingredients[ingredientIndex];
        for (var cardIndex = 0; cardIndex < remainingCards.Count; cardIndex++)
        {
            var card = remainingCards[cardIndex];
            if (!MatchesIngredient(ingredient, card))
            {
                continue;
            }

            remainingCards.RemoveAt(cardIndex);
            if (CanMatchIngredients(
                    ingredients,
                    remainingCards,
                    ingredientIndex + 1))
            {
                return true;
            }

            remainingCards.Insert(cardIndex, card);
        }

        return false;
    }

    private static bool CanCraftWithSelection(
        AbstractRecipeRelic recipe,
        IReadOnlyList<CardModel> selectedIngredients) =>
        selectedIngredients.Count == recipe.Ingredients.Count
        && recipe.Ingredients
            .Select((ingredient, index) =>
                MatchesIngredient(ingredient, selectedIngredients[index]))
            .All(static matches => matches);

    private static bool MatchesIngredient(
        RecipeIngredient ingredient,
        CardModel card) =>
        ingredient.Matches(card)
        || card is FangWeiGu { IsUpgraded: true };
}
