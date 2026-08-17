using System.Reflection;
using STS2RitsuLib;
using STS2RitsuLib.Updates;

namespace GuZhenRen.Systems;

internal static class ModUpdateSystem
{
    private const ulong WorkshopItemId = 3781723792;

    private static readonly Uri BetaManifestUri = new(
        "https://update.miku233.net/beta.json");

    private static readonly Uri WorkshopPageUri = new(
        "https://steamcommunity.com/sharedfiles/filedetails/?id=3781723792");

    private static readonly Uri ManualDownloadUri = new(
        "https://pan.quark.cn/s/6c8152b07310");

    public static IDisposable? Register(Assembly assembly)
    {
        try
        {
            var releasePageUri = RitsuLibFramework.IsAssemblyLoadedFromSteamWorkshopItem(
                assembly,
                WorkshopItemId)
                ? WorkshopPageUri
                : ManualDownloadUri;

            var registration = RitsuLibFramework.RegisterModUpdateCheck(new ModUpdateCheckOptions
            {
                ModId = Entry.ModId,
                DisplayName = "Gu Zhen Ren Beta / 蛊真人 Beta",
                CurrentVersion = Entry.Version,
                ManifestUri = BetaManifestUri,
                ReleasePageUri = releasePageUri,
                InstallSourceAssembly = assembly,
                SteamWorkshopItemId = WorkshopItemId,
            });

            Entry.Logger.Info(
                $"Registered update check for version {Entry.Version} using {BetaManifestUri}.");
            return registration;
        }
        catch (Exception ex)
        {
            Entry.Logger.Warn($"Unable to register the update check: {ex.Message}");
            return null;
        }
    }
}
