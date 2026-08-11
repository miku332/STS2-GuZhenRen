using Godot;
using System.Reflection;
using GuZhenRen.Cards;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.addons.mega_text;
using STS2RitsuLib.Patching.Models;

namespace GuZhenRen.Patches;

public sealed partial class BenMingGuSelectionHeaderPatch : IPatchMethod
{
    private const string MaxHpLossOverlayName =
        "GuZhenRenBenMingGuMaxHpLossOverlay";

    private static readonly FieldInfo CardsField =
        typeof(NChooseACardSelectionScreen).GetField(
            "_cards",
            BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new MissingFieldException(
            typeof(NChooseACardSelectionScreen).FullName,
            "_cards");

    public static string PatchId => "ben-ming-gu-selection-header";

    public static string Description =>
        "Label the opening Ben Ming Gu choice screen clearly";

    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new ModPatchTarget(
            typeof(NChooseACardSelectionScreen),
            nameof(NChooseACardSelectionScreen._Ready),
            Type.EmptyTypes)
    ];

    public static void Postfix(NChooseACardSelectionScreen __instance)
    {
        if (CardsField.GetValue(__instance) is not IReadOnlyList<CardModel> cards
            || cards.Count == 0
            || cards.Any(static card => card is not AbstractBenMingGuCard))
        {
            return;
        }

        Refresh(__instance, cards);
    }

    internal static void Refresh(
        NChooseACardSelectionScreen screen,
        IReadOnlyList<CardModel> cards)
    {
        Clear(screen);

        var banner = screen.GetNode<NCommonBanner>("Banner");
        banner.label.SetTextAutoSize(
            new LocString(
                "card_selection",
                "GU_ZHEN_REN_BEN_MING_GU_SELECTION_HEADER").GetRawText());

        AddMaxHpLossLabels(screen, cards);
    }

    internal static void Clear(NChooseACardSelectionScreen screen)
    {
        if (!GodotObject.IsInstanceValid(screen))
        {
            return;
        }

        var overlays = screen.FindChildren(
                MaxHpLossOverlayName,
                nameof(Control),
                true,
                false)
            .OfType<Control>()
            .ToList();

        foreach (var overlay in overlays)
        {
            overlay.GetParent()?.RemoveChild(overlay);
            overlay.QueueFree();
        }
    }

    private static void AddMaxHpLossLabels(
        NChooseACardSelectionScreen screen,
        IReadOnlyList<CardModel> cards)
    {
        var cardRow = screen.GetNode<Control>("CardRow");
        var text = new LocString(
            "card_selection",
            "GU_ZHEN_REN_BEN_MING_GU_MAX_HP_LOSS").GetFormattedText();

        var overlay = new MaxHpLossOverlay
        {
            Name = MaxHpLossOverlayName,
            Position = Vector2.Zero,
            Size = cardRow.Size,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        cardRow.AddChild(overlay);

        var holders = cardRow.GetChildren().OfType<NGridCardHolder>().ToList();
        for (var index = 0; index < holders.Count; index++)
        {
            var holder = holders[index];
            var label = new MegaLabel
            {
                Name = $"MaxHpLoss{index}",
                Size = new Vector2(300f, 42f),
                Visible = false,
                HorizontalAlignment = HorizontalAlignment.Center,
                MouseFilter = Control.MouseFilterEnum.Ignore,
                Text = text
            };
            // MegaLabel requires an explicit font override before entering the scene tree.
            // Inheriting the holder's font keeps this note consistent with the card UI.
            label.AddThemeFontOverride(
                ThemeConstants.Label.Font,
                holder.GetThemeFont(ThemeConstants.Label.Font));
            label.AddThemeFontSizeOverride(ThemeConstants.Label.FontSize, 30);
            label.AddThemeColorOverride(
                ThemeConstants.Label.FontColor,
                new Color(0.95f, 0.25f, 0.25f));
            label.AddThemeColorOverride(
                ThemeConstants.Label.FontOutlineColor,
                Colors.Black);
            label.AddThemeConstantOverride(ThemeConstants.Label.OutlineSize, 5);
            overlay.AddChild(label);
            overlay.Track(holder, label);
        }
    }

    private sealed partial class MaxHpLossOverlay : Control
    {
        private readonly List<(NGridCardHolder Holder, MegaLabel Label)> _tracked = [];

        public void Track(NGridCardHolder holder, MegaLabel label)
        {
            _tracked.Add((holder, label));
        }

        public override void _Process(double delta)
        {
            var overlayTransform = GetGlobalTransformWithCanvas().AffineInverse();
            foreach (var (holder, label) in _tracked)
            {
                var show = IsInstanceValid(holder)
                    && holder.IsVisibleInTree()
                    && holder.CardModel is LiLiangGu or ZhiHuiGu;
                label.Visible = show;
                if (!show)
                {
                    continue;
                }

                var holderOrigin = overlayTransform
                    * holder.GetGlobalTransformWithCanvas().Origin;
                label.Position = holderOrigin
                    + new Vector2(-label.Size.X / 2f, 216f);
            }
        }
    }
}
