using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.Cards;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class TunHuoPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/TunHuoPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/TunHuoPower_p.png");

    public override async Task AfterCardExhausted(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool causedByEthereal)
    {
        if (card is not Burn
            || card.Owner.Creature != Owner
            || Owner.CombatState is null
            || !Owner.IsAlive
            || Amount <= 0)
        {
            return;
        }

        Flash();
        for (var i = 0; i < Amount; i++)
        {
            var huoShi = Owner.CombatState.CreateCard<HuoShi>(Owner.Player!);
            await CardPileCmd.AddGeneratedCardToCombat(
                huoShi,
                PileType.Hand,
                Owner.Player!,
                CardPilePosition.Bottom);
        }
    }
}
