using System.Reflection;
using System.Text.Json;
using STS2RitsuLib;
using STS2RitsuLib.Updates;

namespace GuZhenRen.Systems;

internal static class ModUpdateSystem
{
    private const ulong WorkshopItemId = 3781723792;

    private static readonly Uri StableManifestUri = new(
        "https://update.miku233.net/stable.json");

    private static readonly Uri WorkshopPageUri = new(
        "https://steamcommunity.com/sharedfiles/filedetails/?id=3781723792");

    private static readonly Uri ManualDownloadUri = new(
        "https://pan.quark.cn/s/6c8152b07310");

    public static IDisposable? Register(Assembly assembly)
    {
        try
        {
            var currentVersion = ReadCurrentVersion(assembly);
            var releasePageUri = RitsuLibFramework.IsAssemblyLoadedFromSteamWorkshopItem(
                assembly,
                WorkshopItemId)
                ? WorkshopPageUri
                : ManualDownloadUri;

            var registration = RitsuLibFramework.RegisterModUpdateCheck(new ModUpdateCheckOptions
            {
                ModId = Entry.ModId,
                DisplayName = "Gu Zhen Ren / 蛊真人",
                CurrentVersion = currentVersion,
                ManifestUri = StableManifestUri,
                ReleasePageUri = releasePageUri,
                InstallSourceAssembly = assembly,
                SteamWorkshopItemId = WorkshopItemId,
            });

            Entry.Logger.Info(
                $"Registered update check for version {currentVersion} using {StableManifestUri}.");
            return registration;
        }
        catch (Exception ex)
        {
            Entry.Logger.Warn($"Unable to register the update check: {ex.Message}");
            return null;
        }
    }

    private static string ReadCurrentVersion(Assembly assembly)
    {
        var assemblyDirectory = Path.GetDirectoryName(assembly.Location);
        if (string.IsNullOrWhiteSpace(assemblyDirectory))
        {
            throw new InvalidOperationException("The mod assembly location is unavailable.");
        }

        var manifestPath = Path.Combine(assemblyDirectory, "mod_manifest.json");
        using var manifest = JsonDocument.Parse(File.ReadAllText(manifestPath));

        if (!manifest.RootElement.TryGetProperty("version", out var versionElement))
        {
            throw new InvalidDataException("mod_manifest.json does not contain a version.");
        }

        var version = versionElement.GetString();
        if (string.IsNullOrWhiteSpace(version))
        {
            throw new InvalidDataException("mod_manifest.json contains an empty version.");
        }

        return version.Trim();
    }
}
