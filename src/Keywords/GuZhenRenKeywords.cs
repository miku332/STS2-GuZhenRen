using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace GuZhenRen.Keywords;

[RegisterOwnedCardKeyword(nameof(ShanYao))]
[RegisterOwnedCardKeyword(nameof(Nian))]
[RegisterOwnedCardKeyword(nameof(Qing))]
[RegisterOwnedCardKeyword(nameof(FenShao))]
[RegisterOwnedCardKeyword(nameof(JiTu))]
[RegisterOwnedCardKeyword(nameof(ZhuanYun))]
[RegisterOwnedCardKeyword(nameof(JianFeng))]
[RegisterOwnedCardKeyword(nameof(JianHen))]
[RegisterOwnedCardKeyword(nameof(JianQi))]
[RegisterOwnedCardKeyword(nameof(GaiLv))]
[RegisterOwnedCardKeyword(nameof(XuYing))]
public sealed class GuZhenRenKeywords
{
    public static readonly CardKeyword ShanYao =
        ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(ShanYao)).GetModCardKeyword();

    public static readonly CardKeyword Nian =
        ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Nian)).GetModCardKeyword();

    public static readonly CardKeyword Qing =
        ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Qing)).GetModCardKeyword();

    public static readonly CardKeyword FenShao =
        ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(FenShao)).GetModCardKeyword();

    public static readonly CardKeyword JiTu =
        ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(JiTu)).GetModCardKeyword();

    public static readonly CardKeyword ZhuanYun =
        ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(ZhuanYun)).GetModCardKeyword();

    public static readonly CardKeyword JianFeng =
        ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(JianFeng)).GetModCardKeyword();

    public static readonly CardKeyword JianHen =
        ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(JianHen)).GetModCardKeyword();

    public static readonly CardKeyword JianQi =
        ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(JianQi)).GetModCardKeyword();

    public static readonly CardKeyword GaiLv =
        ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(GaiLv)).GetModCardKeyword();

    public static readonly CardKeyword XuYing =
        ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(XuYing)).GetModCardKeyword();
}
