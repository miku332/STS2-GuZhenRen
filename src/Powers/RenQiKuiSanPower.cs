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
public sealed class RenQiKuiSanPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/RenQiKuiSanPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/RenQiKuiSanPower_p.png");

    public override decimal ModifyHandDraw(Player player, decimal count) =>
        player.Creature == Owner ? Math.Max(0, count - Amount) : count;

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
