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
            || cardPlay.Card.Tags.Contains(GuZhenRenTags.BianHuaDao)
            || Amount <= 0)
        {
            return;
        }

        if (Owner.GetPower<RuiYiPower>() is not null)
        {
            await ConvertTo<JianDaoDaoHenPower>(cardPlay.Card);
            return;
        }

        if (cardPlay.Card.Tags.Contains(GuZhenRenTags.LiDao))
        {
            await ConvertTo<LiDaoDaoHenPower>(cardPlay.Card);
        }
        else if (cardPlay.Card.Tags.Contains(GuZhenRenTags.YanDao))
        {
            await ConvertTo<YanDaoDaoHenPower>(cardPlay.Card);
        }
        else if (cardPlay.Card.Tags.Contains(GuZhenRenTags.JianDao))
        {
            await ConvertTo<JianDaoDaoHenPower>(cardPlay.Card);
        }
        else if (cardPlay.Card.Tags.Contains(GuZhenRenTags.XueDao))
        {
            await ConvertTo<XueDaoDaoHenPower>(cardPlay.Card);
        }
        else if (cardPlay.Card.Tags.Contains(GuZhenRenTags.GuangDao))
        {
            await ConvertTo<GuangDaoDaoHenPower>(cardPlay.Card);
        }
        else if (cardPlay.Card.Tags.Contains(GuZhenRenTags.FengDao))
        {
            await ConvertTo<FengDaoDaoHenPower>(cardPlay.Card);
        }
        else if (cardPlay.Card.Tags.Contains(GuZhenRenTags.TuDao))
        {
            await ConvertTo<TuDaoDaoHenPower>(cardPlay.Card);
        }
        else if (cardPlay.Card.Tags.Contains(GuZhenRenTags.MuDao))
        {
            await ConvertTo<MuDaoDaoHenPower>(cardPlay.Card);
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
