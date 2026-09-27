using MegaCrit.Sts2.Core.Localization;
using STS2RitsuLib;
using STS2RitsuLib.Data;
using STS2RitsuLib.Settings;
using STS2RitsuLib.Utils.Persistence;

namespace GuZhenRen.Systems;

public sealed class GuZhenRenSettings
{
    public bool ReplaceGuWithQu { get; set; }
}

public static class GuZhenRenSettingsPage
{
    private const string DataKey = "settings";
    private static bool _registered;

    private static readonly ModSettingsCallbackValueBinding<bool>
        ReplaceGuWithQuBinding = new(
            Entry.ModId,
            DataKey,
            SaveScope.Global,
            ReadReplaceGuWithQu,
            WriteReplaceGuWithQu,
            Save);

    public static bool ReplaceGuWithQu => ReadReplaceGuWithQu();

    public static void Register()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;
        ModDataStore.For(Entry.ModId).Register<GuZhenRenSettings>(
            key: DataKey,
            fileName: "settings.json",
            scope: SaveScope.Global,
            defaultFactory: static () => new GuZhenRenSettings(),
            autoCreateIfMissing: true);

        RitsuLibFramework.RegisterModSettings(Entry.ModId, page => page
            .WithTitle(ModSettingsText.Dynamic(
                GetPageTitle,
                ReplaceGuWithQuBinding))
            .WithModDisplayName(ModSettingsText.Dynamic(
                GetPageTitle,
                ReplaceGuWithQuBinding))
            .WithVisibleOnHostSurfaces(
                ModSettingsHostSurface.MainMenu
                | ModSettingsHostSurface.RunPause)
            .AddSection("text", section => section
                .WithTitle(ModSettingsText.Dynamic(GetSectionTitle))
                .AddToggle(
                    "replace_gu_with_qu",
                    ModSettingsText.Dynamic(GetToggleLabel),
                    ReplaceGuWithQuBinding,
                    ModSettingsText.Dynamic(GetToggleDescription))));
    }

    private static bool ReadReplaceGuWithQu()
    {
        try
        {
            return ModDataStore.For(Entry.ModId)
                .Get<GuZhenRenSettings>(DataKey)
                .ReplaceGuWithQu;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    private static void WriteReplaceGuWithQu(bool value)
    {
        ModDataStore.For(Entry.ModId).Modify<GuZhenRenSettings>(
            DataKey,
            settings => settings.ReplaceGuWithQu = value);
        RefreshLocalization();
    }

    private static void Save() =>
        ModDataStore.For(Entry.ModId).Save(DataKey);

    private static void RefreshLocalization()
    {
        var manager = LocManager.Instance;
        if (manager is not null)
        {
            manager.SetLanguage(manager.Language);
        }
    }

    private static bool IsChinese =>
        string.Equals(
            LocManager.Instance?.Language,
            "zhs",
            StringComparison.Ordinal);

    private static string GetPageTitle() => ReplaceGuWithQu
        ? "蛆真人"
        : "蛊真人";

    private static string GetSectionTitle() => IsChinese
        ? "文本显示"
        : "Text Display";

    private static string GetToggleLabel() => IsChinese
        ? "蛆模式"
        : "Maggot Mode";

    private static string GetToggleDescription() => IsChinese
        ? "将本模组中的相关文字替换为彩蛋版本。即时生效，仅影响显示。"
        : "Replaces this mod's related text with its easter-egg version. Applies immediately and only affects display text.";
}
