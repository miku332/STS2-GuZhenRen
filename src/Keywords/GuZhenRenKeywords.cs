using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace GuZhenRen.Keywords;

[RegisterOwnedCardKeyword(nameof(GaiLv))]
[RegisterOwnedCardKeyword(nameof(XuYing))]
[RegisterOwnedCardKeyword(nameof(HuaShi))]
[RegisterOwnedCardKeyword(nameof(AiQingGuPositiveEffect))]
[RegisterOwnedCardKeyword(nameof(AiQingGuNegativeEffect))]
public sealed class GuZhenRenKeywords
{
    public static readonly CardKeyword GaiLv =
        ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(GaiLv)).GetModCardKeyword();

    public static readonly CardKeyword XuYing =
        ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(XuYing)).GetModCardKeyword();

    public static readonly CardKeyword HuaShi =
        ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(HuaShi)).GetModCardKeyword();

    public static readonly CardKeyword AiQingGuPositiveEffect =
        ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(AiQingGuPositiveEffect)).GetModCardKeyword();

    public static readonly CardKeyword AiQingGuNegativeEffect =
        ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(AiQingGuNegativeEffect)).GetModCardKeyword();
}
