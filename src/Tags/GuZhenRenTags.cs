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
[RegisterOwnedCardTag(nameof(MuDao))]
[RegisterOwnedCardTag(nameof(TuDao))]
[RegisterOwnedCardTag(nameof(YanDao))]
[RegisterOwnedCardTag(nameof(XueDao))]
[RegisterOwnedCardTag(nameof(XuYing))]
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

    public static readonly CardTag MuDao =
        ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(MuDao)).GetModCardTag();

    public static readonly CardTag TuDao =
        ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(TuDao)).GetModCardTag();

    public static readonly CardTag YanDao =
        ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(YanDao)).GetModCardTag();

    public static readonly CardTag XueDao =
        ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(XueDao)).GetModCardTag();

    public static readonly CardTag XuYing =
        ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(XuYing)).GetModCardTag();
}
