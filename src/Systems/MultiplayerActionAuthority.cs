using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;

namespace GuZhenRen.Systems;

internal static class MultiplayerActionAuthority
{
    public static bool IsAuthority =>
        RunManager.Instance.NetService.Type != NetGameType.Client;
}
