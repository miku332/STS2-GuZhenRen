using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class FenShenPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/FenShenPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/FenShenPower_p.png");

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (player != Owner.Player || Amount <= 0)
        {
            return;
        }

        Flash();
        await PlayerCmd.GainEnergy((int)Amount, player);

        for (var i = 0; i < Amount; i++)
        {
            var burn = Owner.CombatState!.CreateCard<Burn>(player);
            await CardPileCmd.AddGeneratedCardToCombat(
                burn,
                PileType.Hand,
                player,
                CardPilePosition.Bottom);
        }
    }
}
