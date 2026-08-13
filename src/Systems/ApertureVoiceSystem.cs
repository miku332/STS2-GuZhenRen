using GuZhenRen.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using STS2RitsuLib.Audio;

namespace GuZhenRen.Systems;

internal static class ApertureVoiceSystem
{
    private const string LianTianMoZunPath =
        "res://GuZhenRen/audio/sound/LianTianMoZun.ogg";
    private const string LiuGuanYiPath =
        "res://GuZhenRen/audio/sound/LiuGuanYi.ogg";
    private const string YouHunMoZunPath =
        "res://GuZhenRen/audio/sound/YouHunMoZun.ogg";

    private static readonly string[] VoicePaths =
    [
        LianTianMoZunPath,
        LiuGuanYiPath,
        YouHunMoZunPath
    ];

    public static void Preload()
    {
        foreach (var path in VoicePaths)
        {
            if (!FmodStudioStreamingFiles.TryPreloadResourceAsSound(path))
            {
                Entry.Logger.Warn($"Failed to preload aperture voice '{path}'.");
            }
        }
    }

    public static void PlayForRank(Player player, int rank)
    {
        var path = rank switch
        {
            9 => player.Deck.Cards.OfType<ShaGu>().Any()
                ? YouHunMoZunPath
                : LianTianMoZunPath,
            10 => LiuGuanYiPath,
            _ => null
        };

        if (path is null)
        {
            return;
        }

        if (!FmodStudioStreamingFiles.TryPlayResourceSound(path))
        {
            Entry.Logger.Warn($"Failed to play aperture voice '{path}'.");
        }
    }
}
