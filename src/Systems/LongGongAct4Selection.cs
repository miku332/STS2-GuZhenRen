using Godot;

namespace GuZhenRen.Systems;

internal static class LongGongAct4Selection
{
    private const string ConfigPath = "user://GuZhenRen_settings.cfg";
    private const string Section = "run_options";
    private const string EnabledKey = "enable_long_gong_act4";
    private const string PanelXKey = "long_gong_act4_panel_x";
    private const string PanelYKey = "long_gong_act4_panel_y";

    private static readonly Vector2 DefaultPanelPositionRatio =
        new(1f, 0.04f);

    private static bool _loaded;
    private static bool _enabled = true;
    private static Vector2 _panelPositionRatio = DefaultPanelPositionRatio;

    public static bool Enabled
    {
        get
        {
            EnsureLoaded();
            return _enabled;
        }
        set
        {
            EnsureLoaded();
            if (_enabled == value)
            {
                return;
            }

            _enabled = value;
            Save();
        }
    }

    public static Vector2 PanelPositionRatio
    {
        get
        {
            EnsureLoaded();
            return _panelPositionRatio;
        }
    }

    public static void SavePanelPosition(Vector2 ratio)
    {
        EnsureLoaded();
        _panelPositionRatio = new Vector2(
            Mathf.Clamp(ratio.X, 0f, 1f),
            Mathf.Clamp(ratio.Y, 0f, 1f));
        Save();
    }

    private static void EnsureLoaded()
    {
        if (_loaded)
        {
            return;
        }

        _loaded = true;
        var config = new ConfigFile();
        if (config.Load(ConfigPath) != Error.Ok)
        {
            return;
        }

        _enabled = config.GetValue(Section, EnabledKey, true).AsBool();
        _panelPositionRatio = new Vector2(
            Mathf.Clamp(
                config.GetValue(
                    Section,
                    PanelXKey,
                    DefaultPanelPositionRatio.X).AsSingle(),
                0f,
                1f),
            Mathf.Clamp(
                config.GetValue(
                    Section,
                    PanelYKey,
                    DefaultPanelPositionRatio.Y).AsSingle(),
                0f,
                1f));
    }

    private static void Save()
    {
        var config = new ConfigFile();
        config.Load(ConfigPath);
        config.SetValue(Section, EnabledKey, _enabled);
        config.SetValue(Section, PanelXKey, _panelPositionRatio.X);
        config.SetValue(Section, PanelYKey, _panelPositionRatio.Y);
        var error = config.Save(ConfigPath);
        if (error != Error.Ok)
        {
            Entry.Logger.Warn(
                $"Failed to save Long Gong act option: {error}");
        }
    }
}
