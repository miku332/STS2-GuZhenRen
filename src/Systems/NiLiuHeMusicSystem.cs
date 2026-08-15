using Godot;
using MegaCrit.Sts2.Core.Helpers;
using STS2RitsuLib.Audio;

namespace GuZhenRen.Systems;

internal static class NiLiuHeMusicSystem
{
    private const string MusicPath =
        "res://GuZhenRen/audio/sound/NiLiuHe.ogg";

    private static AudioStreamPlayer? _music;

    public static void Play()
    {
        if (GodotObject.IsInstanceValid(_music) && _music!.Playing)
        {
            return;
        }

        try
        {
            DisposePlayer();

            if (Engine.GetMainLoop() is not SceneTree tree)
            {
                Entry.Logger.Warn("Failed to find the scene tree for Ni Liu He music playback.");
                return;
            }

            var stream = ResourceLoader.Load<AudioStream>(MusicPath);
            if (stream is null)
            {
                Entry.Logger.Warn($"Failed to load Ni Liu He music '{MusicPath}'.");
                return;
            }

            var playerNode = new AudioStreamPlayer
            {
                Stream = stream,
                ProcessMode = Node.ProcessModeEnum.Always
            };
            playerNode.Finished += StopAndRestore;
            tree.Root.AddChild(playerNode);
            _music = playerNode;

            GameFmod.Studio.StopMusic();
            playerNode.Play();
        }
        catch (Exception exception)
        {
            Entry.Logger.Warn(
                $"Failed to play Ni Liu He music '{MusicPath}': " +
                $"{exception.GetType().Name}: {exception.Message}");
            StopAndRestore();
        }
    }

    private static void StopAndRestore()
    {
        DisposePlayer();

        try
        {
            AudioVanillaBridge.RefreshRunMusic();
        }
        catch (Exception exception)
        {
            Entry.Logger.Warn(
                "Failed to restore run music after Ni Liu He playback: " +
                $"{exception.GetType().Name}: {exception.Message}");
        }
    }

    private static void DisposePlayer()
    {
        if (!GodotObject.IsInstanceValid(_music))
        {
            _music = null;
            return;
        }

        _music!.Finished -= StopAndRestore;
        _music.QueueFree();
        _music = null;
    }
}
