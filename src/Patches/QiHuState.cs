using GuZhenRen.Monsters;
using GuZhenRen.Powers;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace GuZhenRen.Patches;

internal static class QiHuState
{
    private static int _bypassDepth;

    public static IDisposable EnterBypassScope()
    {
        _bypassDepth++;
        return new BypassScope();
    }

    public static bool TryRedirectDamage(
        Creature target,
        out Creature redirectedTarget) =>
        TryFindProtector(target, out redirectedTarget);

    public static bool TryRedirectPower(
        PowerModel power,
        Creature target,
        decimal amount,
        out Creature redirectedTarget)
    {
        redirectedTarget = target;
        return power.GetTypeForAmount(amount) == PowerType.Debuff
            && TryFindProtector(target, out redirectedTarget);
    }

    private static bool TryFindProtector(
        Creature target,
        out Creature protector)
    {
        protector = target;
        if (_bypassDepth > 0
            || target.Monster is not LongGong
            || target.CombatState is not { } combatState)
        {
            return false;
        }

        var qiQiang = combatState.Enemies.FirstOrDefault(enemy =>
            enemy.IsAlive
            && !ReferenceEquals(enemy, target)
            && enemy.GetPower<QiHuPower>() is not null);
        if (qiQiang is null)
        {
            return false;
        }

        qiQiang.GetPower<QiHuPower>()?.OnRedirected();
        protector = qiQiang;
        return true;
    }

    private sealed class BypassScope : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _bypassDepth = Math.Max(0, _bypassDepth - 1);
        }
    }
}
