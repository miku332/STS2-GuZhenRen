using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class TianQiKuiSanPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/TianQiKuiSanPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/TianQiKuiSanPower_p.png");

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (player.Creature == Owner && Amount > 0)
        {
            Flash();
            await CreatureCmd.Damage(
                choiceContext,
                Owner,
                6 * Amount,
                ValueProp.Unblockable | ValueProp.Unpowered,
                Applier,
                null);
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
