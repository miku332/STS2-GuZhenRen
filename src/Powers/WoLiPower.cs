using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.Tags;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class WoLiPower : ModPowerTemplate
{
    private readonly HashSet<CardModel> _cardsToConsumeOn =
        new(ReferenceEqualityComparer.Instance);

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/WoLiPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/WoLiPower_p.png");

    public override int ModifyCardPlayCount(
        CardModel card,
        Creature? target,
        int playCount)
    {
        if (Amount <= 0
            || card.Owner.Creature != Owner
            || card.Type != CardType.Attack
            || card.Tags.Contains(GuZhenRenTags.XuYing))
        {
            return playCount;
        }

        _cardsToConsumeOn.Add(card);
        Flash();
        return playCount + 1;
    }

    public override async Task AfterModifyingCardPlayCount(CardModel card)
    {
        if (!_cardsToConsumeOn.Remove(card))
        {
            return;
        }

        await PowerCmd.Decrement(this);
    }
}
