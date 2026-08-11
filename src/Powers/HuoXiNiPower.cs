using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class HuoXiNiPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/HuoXiNiPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/HuoXiNiPower_p.png");

    public override async Task AfterDamageGiven(
        PlayerChoiceContext choiceContext,
        Creature? dealer,
        DamageResult result,
        ValueProp props,
        Creature target,
        CardModel? cardSource)
    {
        if (dealer != Owner
            || target.Player is not { } player
            || Amount <= 0
            || result.TotalDamage <= 0
            || !props.IsPoweredAttack())
        {
            return;
        }

        for (var i = 0; i < Amount; i++)
        {
            var candidates = CardPile.GetCards(
                    player,
                    PileType.Draw,
                    PileType.Discard)
                .ToList();
            if (candidates.Count == 0)
            {
                return;
            }

            Flash();
            var selected = player.RunState.Rng.CombatCardSelection
                .NextItem(candidates)!;
            await CardCmd.Exhaust(choiceContext, selected);
        }
    }
}
