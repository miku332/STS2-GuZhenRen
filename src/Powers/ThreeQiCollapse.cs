using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace GuZhenRen.Powers;

internal static class ThreeQiCollapse
{
    private static readonly HashSet<Creature> Combining = [];

    public static async Task TryCombine(Creature owner, Creature? applier)
    {
        if (owner.GetPower<RenQiKuiSanPower>() is null
            || owner.GetPower<DiQiKuiSanPower>() is null
            || owner.GetPower<TianQiKuiSanPower>() is null
            || !Combining.Add(owner))
        {
            return;
        }

        try
        {
            foreach (var power in new PowerModel?[]
                     {
                         owner.GetPower<RenQiKuiSanPower>(),
                         owner.GetPower<DiQiKuiSanPower>(),
                         owner.GetPower<TianQiKuiSanPower>()
                     }.Where(power => power is not null))
            {
                await PowerCmd.Remove(power!);
            }

            await PowerCmd.Apply<XianQiaoBengKuiPower>(
                new ThrowingPlayerChoiceContext(),
                owner,
                5,
                applier,
                null);
        }
        finally
        {
            Combining.Remove(owner);
        }
    }
}
