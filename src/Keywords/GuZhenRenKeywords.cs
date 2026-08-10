using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace GuZhenRen.Keywords;

[RegisterOwnedCardKeyword(nameof(ShanYao))]
[RegisterOwnedCardKeyword(nameof(Nian))]
public sealed class GuZhenRenKeywords
{
    public static readonly CardKeyword ShanYao =
        ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(ShanYao)).GetModCardKeyword();

    public static readonly CardKeyword Nian =
        ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Nian)).GetModCardKeyword();
}
