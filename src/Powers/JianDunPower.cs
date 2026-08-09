using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.Tags;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class JianDunPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/JianDunPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/JianDunPower_p.png");

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature == Owner
            && RuiYiPower.TreatsAsJianDao(cardPlay.Card))
        {
            await CreatureCmd.GainBlock(
                Owner,
                Amount,
                ValueProp.Unpowered,
                null,
                fast: true);
        }
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Player)
        {
            await PowerCmd.Remove(this);
        }
    }
}
