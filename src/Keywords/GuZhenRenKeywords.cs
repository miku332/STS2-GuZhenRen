using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace GuZhenRen.Keywords;

[RegisterOwnedCardKeyword(nameof(GaiLv))]
[RegisterOwnedCardKeyword(nameof(XuYing))]
public sealed class GuZhenRenKeywords
{
    public static readonly CardKeyword GaiLv =
        ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(GaiLv)).GetModCardKeyword();

    public static readonly CardKeyword XuYing =
        ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(XuYing)).GetModCardKeyword();
}
