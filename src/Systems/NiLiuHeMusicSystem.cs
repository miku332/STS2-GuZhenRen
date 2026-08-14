using STS2RitsuLib.Audio;

namespace GuZhenRen.Systems;

internal static class NiLiuHeMusicSystem
{
    private const string MusicPath =
        "res://GuZhenRen/audio/sound/NiLiuHe.ogg";
    private const string MusicChannel = "GuZhenRen.NiLiuHeMusic";

    private static AudioMusicHandle? _music;

    public static void Preload()
    {
        if (!FmodStudioStreamingFiles.TryPreloadResourceAsStreamingMusic(MusicPath))
        {
            Entry.Logger.Warn($"Failed to preload Ni Liu He music '{MusicPath}'.");
        }
    }

    public static void Play()
    {
        if (_music is { IsValid: true })
        {
            return;
        }

        _music?.Dispose();
        _music = GameAudioService.Shared.PlayMusic(
            AudioSource.StreamingResourceMusic(MusicPath),
            new AudioPlaybackOptions
            {
                Scope = AudioLifecycleScope.Room,
                DebugName = MusicChannel,
                Routing = new AudioRoutingOptions
                {
                    Channel = MusicChannel,
                    ChannelMode = AudioChannelMode.ReplaceExisting
                }
            });

        if (_music is null)
        {
            Entry.Logger.Warn($"Failed to play Ni Liu He music '{MusicPath}'.");
            return;
        }

        GameFmod.Studio.StopMusic();
    }

    public static void Stop()
    {
        _music?.Dispose();
        _music = null;
        AudioVanillaBridge.RefreshRunMusic();
    }
}
