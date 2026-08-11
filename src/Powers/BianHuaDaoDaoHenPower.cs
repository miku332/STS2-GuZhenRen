using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class BianHuaDaoDaoHenPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/BianHuaDaoDaoHenPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/BianHuaDaoDaoHenPower_p.png");

    public override async Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != Owner
            || GuZhenRenTagRules.HasEffectiveTag(
                cardPlay.Card,
                GuZhenRenTags.BianHuaDao)
            || Amount <= 0)
        {
            return;
        }

        if (Owner.GetPower<RuiYiPower>() is not null)
        {
            await ConvertTo<JianDaoDaoHenPower>(cardPlay.Card);
            return;
        }

        if (GuZhenRenTagRules.HasEffectiveTag(cardPlay.Card, GuZhenRenTags.LiDao))
        {
            await ConvertTo<LiDaoDaoHenPower>(cardPlay.Card);
        }
        else if (GuZhenRenTagRules.HasEffectiveTag(cardPlay.Card, GuZhenRenTags.YanDao))
        {
            await ConvertTo<YanDaoDaoHenPower>(cardPlay.Card);
        }
        else if (GuZhenRenTagRules.HasEffectiveTag(cardPlay.Card, GuZhenRenTags.ZhiDao))
        {
            await ConvertTo<ZhiDaoDaoHenPower>(cardPlay.Card);
        }
        else if (GuZhenRenTagRules.HasEffectiveTag(cardPlay.Card, GuZhenRenTags.JianDao))
        {
            await ConvertTo<JianDaoDaoHenPower>(cardPlay.Card);
        }
        else if (GuZhenRenTagRules.HasEffectiveTag(cardPlay.Card, GuZhenRenTags.JinDao))
        {
            await ConvertTo<JinDaoDaoHenPower>(cardPlay.Card);
        }
        else if (GuZhenRenTagRules.HasEffectiveTag(cardPlay.Card, GuZhenRenTags.TouDao))
        {
            await ConvertTo<TouDaoDaoHenPower>(cardPlay.Card);
        }
        else if (GuZhenRenTagRules.HasEffectiveTag(cardPlay.Card, GuZhenRenTags.XueDao))
        {
            await ConvertTo<XueDaoDaoHenPower>(cardPlay.Card);
        }
        else if (GuZhenRenTagRules.HasEffectiveTag(cardPlay.Card, GuZhenRenTags.GuangDao))
        {
            await ConvertTo<GuangDaoDaoHenPower>(cardPlay.Card);
        }
        else if (GuZhenRenTagRules.HasEffectiveTag(cardPlay.Card, GuZhenRenTags.FengDao))
        {
            await ConvertTo<FengDaoDaoHenPower>(cardPlay.Card);
        }
        else if (GuZhenRenTagRules.HasEffectiveTag(cardPlay.Card, GuZhenRenTags.TuDao))
        {
            await ConvertTo<TuDaoDaoHenPower>(cardPlay.Card);
        }
        else if (GuZhenRenTagRules.HasEffectiveTag(cardPlay.Card, GuZhenRenTags.MuDao))
        {
            await ConvertTo<MuDaoDaoHenPower>(cardPlay.Card);
        }
        else if (GuZhenRenTagRules.HasEffectiveTag(cardPlay.Card, GuZhenRenTags.GuDao))
        {
            await ConvertTo<GuDaoDaoHenPower>(cardPlay.Card);
        }
        else if (GuZhenRenTagRules.HasEffectiveTag(cardPlay.Card, GuZhenRenTags.LuDao))
        {
            await ConvertTo<LuDaoDaoHenPower>(cardPlay.Card);
        }
        else if (GuZhenRenTagRules.HasEffectiveTag(cardPlay.Card, GuZhenRenTags.ShiDao))
        {
            await ConvertTo<ShiDaoDaoHenPower>(cardPlay.Card);
        }
        else if (GuZhenRenTagRules.HasEffectiveTag(cardPlay.Card, GuZhenRenTags.YunDao))
        {
            await ConvertTo<YunDaoDaoHenPower>(cardPlay.Card);
        }
        else if (GuZhenRenTagRules.HasEffectiveTag(cardPlay.Card, GuZhenRenTags.ZhouDao))
        {
            await ConvertTo<ZhouDaoDaoHenPower>(cardPlay.Card);
        }
        else if (GuZhenRenTagRules.HasEffectiveTag(cardPlay.Card, GuZhenRenTags.ShaDao))
        {
            await ConvertTo<ShaDaoDaoHenPower>(cardPlay.Card);
        }
    }

    private async Task ConvertTo<TPower>(CardModel cardSource)
        where TPower : AbstractDaoHenPower
    {
        Flash();
        var amount = Amount;
        await PowerCmd.Remove(this);
        await PowerCmd.Apply<TPower>(
            new ThrowingPlayerChoiceContext(),
            Owner,
            amount,
            Owner,
            cardSource);
        await ZhuanYiPower.TriggerConversion(Owner, Owner, cardSource);
    }
}
