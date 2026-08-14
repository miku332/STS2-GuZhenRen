using Godot;
using MegaCrit.Sts2.Core.Helpers;
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
                Scope = AudioLifecycleScope.Run,
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
        TaskHelper.RunSafely(RestoreMusicAfterPlayback());
    }

    private static async Task RestoreMusicAfterPlayback()
    {
        if (Engine.GetMainLoop() is not SceneTree tree)
        {
            Entry.Logger.Warn("Failed to schedule Ni Liu He music restoration.");
            return;
        }

        var stream = ResourceLoader.Load<AudioStream>(MusicPath);
        var duration = stream?.GetLength() ?? 34.3;
        await tree.ToSignal(
            tree.CreateTimer(duration, processAlways: true, ignoreTimeScale: true),
            SceneTreeTimer.SignalName.Timeout);

        StopAndRestore();
    }

    private static void StopAndRestore()
    {
        _music?.Dispose();
        _music = null;
        AudioVanillaBridge.RefreshRunMusic();
    }
}
