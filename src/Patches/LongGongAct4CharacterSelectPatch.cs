using Godot;
using GuZhenRen.Acts;
using GuZhenRen.Characters;
using GuZhenRen.Systems;
using HarmonyLib;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;

namespace GuZhenRen.Patches;

[HarmonyPatch]
internal static partial class LongGongAct4CharacterSelectPatch
{
    private const string PanelName = "GuZhenRenLongGongAct4Option";
    private const string IconPath =
        "res://GuZhenRen/images/map/long_gong_boss.png";

    [HarmonyPatch(typeof(NCharacterSelectScreen), nameof(NCharacterSelectScreen._Ready))]
    [HarmonyPostfix]
    private static void PostfixReady(NCharacterSelectScreen __instance)
    {
        if (__instance.GetNodeOrNull<Control>(PanelName) is not null)
        {
            return;
        }

        var option = CreateOption();
        __instance.AddChild(option.Root);
        option.Refresh(LongGongAct4Selection.Enabled);
        option.Root.Visible = false;
    }

    [HarmonyPatch(
        typeof(NCharacterSelectScreen),
        nameof(NCharacterSelectScreen.SelectCharacter))]
    [HarmonyPostfix]
    private static void PostfixSelectCharacter(
        NCharacterSelectScreen __instance,
        CharacterModel characterModel)
    {
        var root = __instance.GetNodeOrNull<Control>(PanelName);
        if (root is null)
        {
            return;
        }

        root.Visible = characterModel is FangYuanCharacter
            && __instance.Lobby.NetService.Type == NetGameType.Singleplayer;
    }

    [HarmonyPatch(typeof(NCharacterSelectScreen), nameof(NCharacterSelectScreen.BeginRun))]
    [HarmonyPrefix]
    private static void PrefixBeginRun(
        NCharacterSelectScreen __instance,
        List<ActModel> acts)
    {
        if (__instance.Lobby.NetService.Type != NetGameType.Singleplayer
            || __instance.Lobby.LocalPlayer.character is not FangYuanCharacter)
        {
            return;
        }

        acts.RemoveAll(static act => act is GuZhenRenFinalAct);
        if (LongGongAct4Selection.Enabled)
        {
            acts.Add(ModelDb.Act<GuZhenRenFinalAct>());
            Entry.Logger.Info("Enabled Long Gong final act for this run.");
        }
        else
        {
            Entry.Logger.Info("Disabled Long Gong final act for this run.");
        }
    }

    private static Act4Option CreateOption()
    {
        var root = new DraggableAct4Panel
        {
            Name = PanelName,
            CustomMinimumSize = new Vector2(580f, 142f),
            Size = new Vector2(580f, 142f),
            SizeFlagsHorizontal = Control.SizeFlags.ShrinkBegin,
            SizeFlagsVertical = Control.SizeFlags.ShrinkBegin,
            MouseFilter = Control.MouseFilterEnum.Stop,
            MouseDefaultCursorShape = Control.CursorShape.Move
        };

        var option = new Act4Option(root);

        var background = new Panel
        {
            MouseFilter = Control.MouseFilterEnum.Ignore,
            AnchorRight = 1f,
            AnchorBottom = 1f
        };
        root.AddChild(background);

        var margin = new MarginContainer
        {
            MouseFilter = Control.MouseFilterEnum.Ignore,
            AnchorRight = 1f,
            AnchorBottom = 1f
        };
        margin.AddThemeConstantOverride("margin_left", 18);
        margin.AddThemeConstantOverride("margin_right", 18);
        margin.AddThemeConstantOverride("margin_top", 14);
        margin.AddThemeConstantOverride("margin_bottom", 14);
        root.AddChild(margin);

        var row = new HBoxContainer
        {
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        row.AddThemeConstantOverride("separation", 16);
        margin.AddChild(row);

        var icon = new TextureRect
        {
            CustomMinimumSize = new Vector2(82f, 82f),
            Texture = PreloadManager.Cache.GetTexture2D(IconPath),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        row.AddChild(icon);

        var textColumn = new VBoxContainer
        {
            SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        textColumn.AddThemeConstantOverride("separation", 1);
        row.AddChild(textColumn);

        var kicker = CreateLabel(
            Loc("GU_ZHEN_REN_LONG_GONG_ACT4.kicker"),
            16,
            new Color(0.82f, 0.66f, 0.30f));
        textColumn.AddChild(kicker);

        var title = CreateLabel(
            Loc("GU_ZHEN_REN_LONG_GONG_ACT4.title"),
            26,
            new Color(1f, 0.88f, 0.52f));
        textColumn.AddChild(title);

        var description = CreateLabel(
            Loc("GU_ZHEN_REN_LONG_GONG_ACT4.description"),
            17,
            new Color(0.84f, 0.84f, 0.88f));
        description.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        textColumn.AddChild(description);

        var toggle = new Button
        {
            CustomMinimumSize = new Vector2(92f, 0f),
            ToggleMode = true,
            ButtonPressed = LongGongAct4Selection.Enabled,
            FocusMode = Control.FocusModeEnum.All,
            MouseFilter = Control.MouseFilterEnum.Stop,
            MouseDefaultCursorShape = Control.CursorShape.PointingHand
        };
        var emptyStyle = new StyleBoxEmpty();
        toggle.AddThemeStyleboxOverride("normal", emptyStyle);
        toggle.AddThemeStyleboxOverride("hover", emptyStyle);
        toggle.AddThemeStyleboxOverride("pressed", emptyStyle);
        toggle.AddThemeStyleboxOverride("hover_pressed", emptyStyle);
        toggle.AddThemeStyleboxOverride("focus", emptyStyle);
        row.AddChild(toggle);

        var stateColumn = new VBoxContainer
        {
            AnchorRight = 1f,
            AnchorBottom = 1f,
            Alignment = BoxContainer.AlignmentMode.Center,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        stateColumn.AddThemeConstantOverride("separation", 7);
        toggle.AddChild(stateColumn);

        var track = new Panel
        {
            CustomMinimumSize = new Vector2(70f, 36f),
            SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        stateColumn.AddChild(track);

        var knob = new Panel
        {
            Position = new Vector2(5f, 5f),
            Size = new Vector2(26f, 26f),
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        knob.AddThemeStyleboxOverride("panel", CreateKnobStyle());
        track.AddChild(knob);

        var stateLabel = CreateLabel(string.Empty, 16, Colors.White);
        stateLabel.HorizontalAlignment = HorizontalAlignment.Center;
        stateColumn.AddChild(stateLabel);

        option.Track = track;
        option.Knob = knob;
        option.StateLabel = stateLabel;
        option.Toggle = toggle;
        option.Background = background;
        toggle.Toggled += enabled =>
        {
            LongGongAct4Selection.Enabled = enabled;
            option.Refresh(enabled);
        };
        return option;
    }

    private static Label CreateLabel(string text, int size, Color color)
    {
        var label = new Label
        {
            Text = text,
            MouseFilter = Control.MouseFilterEnum.Ignore
        };
        label.AddThemeFontSizeOverride("font_size", size);
        label.AddThemeColorOverride("font_color", color);
        label.AddThemeColorOverride("font_outline_color", Colors.Black);
        label.AddThemeConstantOverride("outline_size", 4);
        return label;
    }

    private static string Loc(string key) =>
        new LocString("characters", key).GetFormattedText();

    private static StyleBoxFlat CreatePanelStyle(bool enabled, bool hover)
    {
        var style = new StyleBoxFlat
        {
            BgColor = enabled
                ? new Color(0.09f, 0.075f, 0.055f, hover ? 0.98f : 0.94f)
                : new Color(0.045f, 0.045f, 0.055f, hover ? 0.98f : 0.94f),
            BorderColor = enabled
                ? new Color(0.86f, 0.65f, 0.25f, 0.95f)
                : new Color(0.34f, 0.34f, 0.40f, 0.9f),
            BorderWidthLeft = 2,
            BorderWidthTop = 2,
            BorderWidthRight = 2,
            BorderWidthBottom = 2,
            CornerRadiusTopLeft = 6,
            CornerRadiusTopRight = 6,
            CornerRadiusBottomLeft = 6,
            CornerRadiusBottomRight = 6,
            ShadowColor = new Color(0f, 0f, 0f, 0.55f),
            ShadowSize = hover ? 12 : 8
        };
        return style;
    }

    private static StyleBoxFlat CreateTrackStyle(bool enabled) => new()
    {
        BgColor = enabled
            ? new Color(0.55f, 0.38f, 0.10f)
            : new Color(0.20f, 0.20f, 0.24f),
        BorderColor = enabled
            ? new Color(1f, 0.78f, 0.30f)
            : new Color(0.42f, 0.42f, 0.48f),
        BorderWidthLeft = 2,
        BorderWidthTop = 2,
        BorderWidthRight = 2,
        BorderWidthBottom = 2,
        CornerRadiusTopLeft = 18,
        CornerRadiusTopRight = 18,
        CornerRadiusBottomLeft = 18,
        CornerRadiusBottomRight = 18
    };

    private static StyleBoxFlat CreateKnobStyle() => new()
    {
        BgColor = new Color(1f, 0.92f, 0.68f),
        CornerRadiusTopLeft = 13,
        CornerRadiusTopRight = 13,
        CornerRadiusBottomLeft = 13,
        CornerRadiusBottomRight = 13,
        ShadowColor = new Color(0f, 0f, 0f, 0.5f),
        ShadowSize = 3
    };

    private sealed class Act4Option(DraggableAct4Panel root)
    {
        public DraggableAct4Panel Root { get; } = root;
        public Button Toggle { get; set; } = null!;
        public Panel Background { get; set; } = null!;
        public Panel Track { get; set; } = null!;
        public Panel Knob { get; set; } = null!;
        public Label StateLabel { get; set; } = null!;

        public void Refresh(bool enabled)
        {
            Toggle.SetPressedNoSignal(enabled);
            Background.AddThemeStyleboxOverride(
                "panel", CreatePanelStyle(enabled, hover: false));
            Track.AddThemeStyleboxOverride("panel", CreateTrackStyle(enabled));
            Knob.Position = new Vector2(enabled ? 39f : 5f, 5f);
            StateLabel.Text = Loc(enabled
                ? "GU_ZHEN_REN_LONG_GONG_ACT4.enabled"
                : "GU_ZHEN_REN_LONG_GONG_ACT4.disabled");
            StateLabel.AddThemeColorOverride(
                "font_color",
                enabled
                    ? new Color(1f, 0.82f, 0.38f)
                    : new Color(0.68f, 0.68f, 0.72f));
        }
    }

    private sealed partial class DraggableAct4Panel : Control
    {
        private const float ScreenMargin = 24f;

        private bool _dragging;
        private Vector2 _dragOffset;
        private Viewport? _viewport;

        public override void _Ready()
        {
            _viewport = GetViewport();
            _viewport.SizeChanged += ApplySavedPosition;
            ApplySavedPosition();
        }

        public override void _ExitTree()
        {
            if (_viewport is not null && IsInstanceValid(_viewport))
            {
                _viewport.SizeChanged -= ApplySavedPosition;
            }
        }

        public override void _GuiInput(InputEvent inputEvent)
        {
            switch (inputEvent)
            {
                case InputEventMouseButton mouseButton
                    when mouseButton.ButtonIndex == MouseButton.Left:
                    _dragging = mouseButton.Pressed;
                    if (_dragging)
                    {
                        _dragOffset = mouseButton.GlobalPosition - GlobalPosition;
                    }
                    else
                    {
                        SavePosition();
                    }

                    AcceptEvent();
                    break;

                case InputEventMouseMotion mouseMotion when _dragging:
                    Position = ClampPosition(
                        mouseMotion.GlobalPosition - _dragOffset);
                    AcceptEvent();
                    break;
            }
        }

        private void ApplySavedPosition()
        {
            var available = GetAvailableTravel();
            var ratio = LongGongAct4Selection.PanelPositionRatio;
            Position = ClampPosition(
                new Vector2(
                    ScreenMargin + (available.X * ratio.X),
                    ScreenMargin + (available.Y * ratio.Y)));
        }

        private void SavePosition()
        {
            Position = ClampPosition(Position);
            var available = GetAvailableTravel();
            LongGongAct4Selection.SavePanelPosition(
                new Vector2(
                    available.X <= 0f
                        ? 0f
                        : (Position.X - ScreenMargin) / available.X,
                    available.Y <= 0f
                        ? 0f
                        : (Position.Y - ScreenMargin) / available.Y));
        }

        private Vector2 ClampPosition(Vector2 position)
        {
            var bounds = GetLayoutBounds();
            return new Vector2(
                Mathf.Clamp(
                    position.X,
                    ScreenMargin,
                    Mathf.Max(ScreenMargin, bounds.X - Size.X - ScreenMargin)),
                Mathf.Clamp(
                    position.Y,
                    ScreenMargin,
                    Mathf.Max(ScreenMargin, bounds.Y - Size.Y - ScreenMargin)));
        }

        private Vector2 GetAvailableTravel()
        {
            var bounds = GetLayoutBounds();
            return new Vector2(
                Mathf.Max(0f, bounds.X - Size.X - (ScreenMargin * 2f)),
                Mathf.Max(0f, bounds.Y - Size.Y - (ScreenMargin * 2f)));
        }

        private Vector2 GetLayoutBounds()
        {
            if (GetParent() is Control parent && parent.Size.X > 0f && parent.Size.Y > 0f)
            {
                return parent.Size;
            }

            return GetViewportRect().Size;
        }
    }
}
