using GuZhenRen.Cards;
using GuZhenRen.Enchantments;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace GuZhenRen.Tags;

public static class GuZhenRenTagRules
{
    private static readonly HashSet<CardTag> DaoTags =
    [
        GuZhenRenTags.BianHuaDao,
        GuZhenRenTags.FengDao,
        GuZhenRenTags.GuangDao,
        GuZhenRenTags.GuDao,
        GuZhenRenTags.JianDao,
        GuZhenRenTags.LiDao,
        GuZhenRenTags.LuDao,
        GuZhenRenTags.MuDao,
        GuZhenRenTags.ShaDao,
        GuZhenRenTags.ShiDao,
        GuZhenRenTags.TuDao,
        GuZhenRenTags.XueDao,
        GuZhenRenTags.YanDao,
        GuZhenRenTags.ZhiDao,
        GuZhenRenTags.YunDao,
        GuZhenRenTags.ZhouDao
    ];

    public static bool HasEffectiveTag(CardModel card, CardTag tag)
    {
        if (card is GuZhenRenCardTemplate
            && card.Enchantment is HuaShiEnchantment
            && DaoTags.Contains(tag))
        {
            return tag == GuZhenRenTags.TuDao;
        }

        return card.Tags.Contains(tag);
    }

    public static IEnumerable<CardTag> GetEffectiveDaoTags(CardModel card)
    {
        if (card is GuZhenRenCardTemplate
            && card.Enchantment is HuaShiEnchantment)
        {
            return [GuZhenRenTags.TuDao];
        }

        return card.Tags.Where(DaoTags.Contains);
    }
}
