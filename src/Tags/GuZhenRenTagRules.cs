using GuZhenRen.Cards;
using GuZhenRen.Enchantments;
using GuZhenRen.Powers;
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
        GuZhenRenTags.GuChongDao,
        GuZhenRenTags.JianDao,
        GuZhenRenTags.JinDao,
        GuZhenRenTags.LiDao,
        GuZhenRenTags.LuDao,
        GuZhenRenTags.MuDao,
        GuZhenRenTags.ShaDao,
        GuZhenRenTags.ShiDao,
        GuZhenRenTags.TuDao,
        GuZhenRenTags.TouDao,
        GuZhenRenTags.XueDao,
        GuZhenRenTags.YanDao,
        GuZhenRenTags.ZhiDao,
        GuZhenRenTags.YunDao,
        GuZhenRenTags.ZhouDao
    ];

    public static bool HasEffectiveTag(CardModel card, CardTag tag)
    {
        if (DaoTags.Contains(tag))
        {
            return GetEffectiveDaoTags(card).Contains(tag);
        }

        return card.Tags.Contains(tag);
    }

    public static IEnumerable<CardTag> GetEffectiveDaoTags(CardModel card)
    {
        var effectiveTags = card is GuZhenRenCardTemplate
            && card.Enchantment is HuaShiEnchantment
            ? [GuZhenRenTags.TuDao]
            : card.Tags.Where(DaoTags.Contains).ToArray();

        return effectiveTags.Length > 0
            && card.Owner?.Creature.GetPower<RuiYiPower>() is not null
                ? [GuZhenRenTags.JianDao]
                : effectiveTags;
    }
}
