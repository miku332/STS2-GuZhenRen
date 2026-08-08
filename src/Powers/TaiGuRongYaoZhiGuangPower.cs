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
public sealed class TaiGuRongYaoZhiGuangPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/TaiGuRongYaoZhiGuangPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/TaiGuRongYaoZhiGuangPower_p.png");

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner
            || dealer?.Player is null
            || cardSource is null
            || !cardSource.Tags.Contains(GuZhenRenTags.GuangDao)
            || result.TotalDamage <= 0)
        {
            return;
        }

        for (var i = 0; i < Amount; i++)
        {
            await CreatureCmd.Damage(
                choiceContext,
                Owner,
                result.TotalDamage,
                ValueProp.Unpowered,
                dealer,
                null);
        }
    }
}
