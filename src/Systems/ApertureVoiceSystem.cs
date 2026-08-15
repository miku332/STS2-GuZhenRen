using GuZhenRen.Cards;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;

namespace GuZhenRen.Systems;

internal static class ApertureVoiceSystem
{
    private const string LianTianMoZunPath =
        "res://GuZhenRen/audio/sound/LianTianMoZun.ogg";
    private const string LiuGuanYiPath =
        "res://GuZhenRen/audio/sound/LiuGuanYi.ogg";
    private const string YouHunMoZunPath =
        "res://GuZhenRen/audio/sound/YouHunMoZun.ogg";

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

        try
        {
            if (Engine.GetMainLoop() is not SceneTree tree)
            {
                Entry.Logger.Warn("Failed to find the scene tree for aperture voice playback.");
                return;
            }

            var stream = ResourceLoader.Load<AudioStream>(path);
            if (stream is null)
            {
                Entry.Logger.Warn($"Failed to load aperture voice '{path}'.");
                return;
            }

            var playerNode = new AudioStreamPlayer
            {
                Stream = stream,
                ProcessMode = Node.ProcessModeEnum.Always
            };
            playerNode.Finished += playerNode.QueueFree;
            tree.Root.AddChild(playerNode);
            playerNode.Play();
        }
        catch (Exception exception)
        {
            Entry.Logger.Warn(
                $"Failed to play aperture voice '{path}': " +
                $"{exception.GetType().Name}: {exception.Message}");
        }
    }
}
