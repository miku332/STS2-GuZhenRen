using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class RuiYiPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.None;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/RuiYiPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/RuiYiPower_p.png");

    public static bool TreatsAsJianDao(CardModel card) =>
        card.Tags.Contains(GuZhenRenTags.JianDao)
        || card.Owner.Creature.GetPower<RuiYiPower>() is not null;
}
