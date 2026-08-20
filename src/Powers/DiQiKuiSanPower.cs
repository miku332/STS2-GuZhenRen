using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class DiQiKuiSanPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/DiQiKuiSanPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/DiQiKuiSanPower_p.png");

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (player.Creature == Owner && Amount > 0)
        {
            Flash();
            await PlayerCmd.LoseEnergy(Amount, player);
        }
    }

    public override Task AfterApplied(Creature? applier, CardModel? cardSource) =>
        ThreeQiCollapse.TryCombine(Owner, applier);

    public override Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal amount,
        Creature? applier,
        CardModel? cardSource) =>
        power == this
            ? ThreeQiCollapse.TryCombine(Owner, applier)
            : Task.CompletedTask;
}
