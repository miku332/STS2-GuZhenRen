using GuZhenRen.Acts;
using MegaCrit.Sts2.Core.Commands;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;

namespace GuZhenRen.Systems;

[RegisterSingleton]
public sealed class HeavenlyCourtRecovery
    : HookedSingletonModel
{
    public HeavenlyCourtRecovery()
        : base(HookType.Run)
    {
    }

    public override async Task AfterActEntered()
    {
        if (CurrentRunState?.Act is not GuZhenRenFinalAct)
        {
            return;
        }

        foreach (var player in CurrentRunState.Players)
        {
            var creature = player.Creature;
            var missingHp = creature.MaxHp - creature.CurrentHp;
            if (missingHp > 0)
            {
                await CreatureCmd.Heal(creature, missingHp);
            }
        }
    }
}
