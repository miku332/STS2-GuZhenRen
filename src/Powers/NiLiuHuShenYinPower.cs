using GuZhenRen.Relics;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class NiLiuHuShenYinPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/NiLiuHuShenYinPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/NiLiuHuShenYinPower_p.png");

    public override Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (player != Owner.Player || Amount <= 0)
        {
            return Task.CompletedTask;
        }

        var relic = player.GetRelic<NiLiuHe>();
        if (relic is not null && relic.AddWater(Amount))
        {
            Flash();
        }

        return Task.CompletedTask;
    }
}
