using System.Reflection;
using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using STS2RitsuLib.Patching.Models;

namespace GuZhenRen.Patches;

public sealed class ShaZhaoRecipeSelectionBackPatch : IPatchMethod
{
    private const string BackButtonName = "GuZhenRenRecipeBackButton";
    private const string BackButtonScenePath =
        "res://scenes/ui/back_button.tscn";

    private static readonly FieldInfo? PrefsField =
        typeof(NSimpleCardSelectScreen).GetField(
            "_prefs",
            BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly FieldInfo? SelectedCardsField =
        typeof(NSimpleCardSelectScreen).GetField(
            "_selectedCards",
            BindingFlags.Instance | BindingFlags.NonPublic);

    public static string PatchId => "sha-zhao-recipe-selection-back";

    public static string Description =>
        "Add a native back button to the Sha Zhao recipe selection screen";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(
            typeof(NSimpleCardSelectScreen),
            nameof(NSimpleCardSelectScreen._Ready),
            Type.EmptyTypes)
    ];

    public static void Postfix(NSimpleCardSelectScreen __instance)
    {
        if (PrefsField?.GetValue(__instance) is not CardSelectorPrefs prefs
            || prefs.Prompt.LocTable != "card_selection"
            || prefs.Prompt.LocEntryKey
                != "GU_ZHEN_REN_ASSEMBLE_CHOOSE_RECIPE"
            || __instance.HasNode(BackButtonName))
        {
            return;
        }

        var scene = GD.Load<PackedScene>(BackButtonScenePath);
        if (scene is null)
        {
            Entry.Logger.Warn("Unable to load the native recipe back button.");
            return;
        }

        var backButton = scene.Instantiate<NBackButton>();
        backButton.Name = BackButtonName;
        __instance.AddChild(backButton);
        backButton.Connect(
            NClickableControl.SignalName.Released,
            Callable.From<NButton>(_ =>
            {
                if (GodotObject.IsInstanceValid(__instance))
                {
                    if (SelectedCardsField?.GetValue(__instance)
                        is HashSet<CardModel> selectedCards)
                    {
                        selectedCards.Clear();
                    }

                    __instance.Call(
                        NSimpleCardSelectScreen.MethodName.CompleteSelection);
                }
            }));
        backButton.Enable();
    }
}

public sealed class ShaZhaoRecipeSelectionReplacePatch : IPatchMethod
{
    private static readonly FieldInfo? PrefsField =
        typeof(NSimpleCardSelectScreen).GetField(
            "_prefs",
            BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly FieldInfo? SelectedCardsField =
        typeof(NSimpleCardSelectScreen).GetField(
            "_selectedCards",
            BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly FieldInfo? GridField =
        typeof(NCardGridSelectionScreen).GetField(
            "_grid",
            BindingFlags.Instance | BindingFlags.NonPublic);

    public static string PatchId => "sha-zhao-recipe-selection-replace";

    public static string Description =>
        "Allow one-click replacement on the Sha Zhao recipe selection screen";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(
            typeof(NSimpleCardSelectScreen),
            "OnCardClicked",
            [typeof(CardModel)])
    ];

    public static void Prefix(
        NSimpleCardSelectScreen __instance,
        CardModel card)
    {
        if (PrefsField?.GetValue(__instance) is not CardSelectorPrefs prefs
            || prefs.Prompt.LocTable != "card_selection"
            || prefs.Prompt.LocEntryKey
                != "GU_ZHEN_REN_ASSEMBLE_CHOOSE_RECIPE"
            || prefs.MaxSelect != 1
            || SelectedCardsField?.GetValue(__instance)
                is not HashSet<CardModel> selectedCards
            || selectedCards.Count == 0
            || selectedCards.Contains(card))
        {
            return;
        }

        if (GridField?.GetValue(__instance) is NCardGrid grid)
        {
            foreach (var selectedCard in selectedCards)
            {
                grid.UnhighlightCard(selectedCard);
            }
        }

        selectedCards.Clear();
    }
}
