using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.CardTags;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;

namespace GuZhenRen.Tags;

[RegisterOwnedCardTag(nameof(BianHuaDao))]
[RegisterOwnedCardTag(nameof(GuangDao))]
[RegisterOwnedCardTag(nameof(FengDao))]
[RegisterOwnedCardTag(nameof(JianDao))]
[RegisterOwnedCardTag(nameof(LiDao))]
[RegisterOwnedCardTag(nameof(LuDao))]
[RegisterOwnedCardTag(nameof(MuDao))]
[RegisterOwnedCardTag(nameof(ShaDao))]
[RegisterOwnedCardTag(nameof(ShiDao))]
[RegisterOwnedCardTag(nameof(TuDao))]
[RegisterOwnedCardTag(nameof(YanDao))]
[RegisterOwnedCardTag(nameof(XueDao))]
[RegisterOwnedCardTag(nameof(XuYing))]
[RegisterOwnedCardTag(nameof(ZhiDao))]
[RegisterOwnedCardTag(nameof(YunDao))]
public sealed class GuZhenRenTags
{
    public static readonly CardTag BianHuaDao =
        ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(BianHuaDao)).GetModCardTag();

    public static readonly CardTag FengDao =
        ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(FengDao)).GetModCardTag();

    public static readonly CardTag GuangDao =
        ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(GuangDao)).GetModCardTag();

    public static readonly CardTag JianDao =
        ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(JianDao)).GetModCardTag();

    public static readonly CardTag LiDao =
        ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(LiDao)).GetModCardTag();

    public static readonly CardTag LuDao =
        ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(LuDao)).GetModCardTag();

    public static readonly CardTag MuDao =
        ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(MuDao)).GetModCardTag();

    public static readonly CardTag ShaDao =
        ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(ShaDao)).GetModCardTag();

    public static readonly CardTag ShiDao =
        ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(ShiDao)).GetModCardTag();

    public static readonly CardTag TuDao =
        ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(TuDao)).GetModCardTag();

    public static readonly CardTag YanDao =
        ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(YanDao)).GetModCardTag();

    public static readonly CardTag XueDao =
        ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(XueDao)).GetModCardTag();

    public static readonly CardTag XuYing =
        ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(XuYing)).GetModCardTag();

    public static readonly CardTag ZhiDao =
        ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(ZhiDao)).GetModCardTag();

    public static readonly CardTag YunDao =
        ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(YunDao)).GetModCardTag();
}
