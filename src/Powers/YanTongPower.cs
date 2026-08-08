using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class YanTongPower : ModPowerTemplate
{
    private const int BurnThreshold = 4;

    private int _triggerCount;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/YanTongPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/YanTongPower_p.png");

    public override async Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != Owner
            || cardPlay.Target is null
            || cardPlay.Target == Owner
            || !cardPlay.Target.IsAlive)
        {
            return;
        }

        Flash();
        _triggerCount++;

        await PowerCmd.Apply<FenShaoPower>(
            choiceContext,
            cardPlay.Target,
            Amount,
            Owner,
            cardPlay.Card);

        if (_triggerCount > BurnThreshold)
        {
            var burn = Owner.CombatState!.CreateCard<Burn>(Owner.Player!);
            await CardPileCmd.AddGeneratedCardToCombat(
                burn,
                PileType.Hand,
                Owner.Player!,
                CardPilePosition.Bottom);
        }
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (participants.Contains(Owner))
        {
            await PowerCmd.Remove(this);
        }
    }
}
