using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.Tags;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class JianFengPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/JianFengPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/JianFengPower_p.png");

    public override async Task AfterDamageGiven(
        PlayerChoiceContext choiceContext,
        Creature? dealer,
        DamageResult result,
        ValueProp props,
        Creature target,
        CardModel? cardSource)
    {
        if (dealer != Owner
            || target == Owner
            || cardSource is null
            || !RuiYiPower.TreatsAsJianDao(cardSource)
            || !props.IsPoweredAttack()
            || result.TotalDamage <= 0)
        {
            return;
        }

        await PowerCmd.Apply<JianHenPower>(
            choiceContext,
            target,
            Amount,
            Owner,
            null);
    }
}
