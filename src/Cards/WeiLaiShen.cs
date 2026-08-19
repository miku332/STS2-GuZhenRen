using GuZhenRen.CardPools;
using GuZhenRen.Relics;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models.Capabilities;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class WeiLaiShen : AbstractShaZhaoCard
{
    public const int Duration = 3;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/WeiLaiShen.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.ZhouDao];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Battles", Duration)
    ];

    public WeiLaiShen()
        : base(3, CardType.Skill, CardRarity.Token, TargetType.None, false)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        UpgradeCombatCards();

        var deckCard = DeckVersion as WeiLaiShen;
        if (deckCard is null && Pile?.Type == PileType.Deck)
        {
            deckCard = this;
        }

        deckCard ??= Owner.Deck.Cards.OfType<WeiLaiShen>().FirstOrDefault();
        if (deckCard?.Pile?.Type == PileType.Deck)
        {
            await CardPileCmd.RemoveFromDeck(deckCard, showPreview: false);
        }

        var relic = Owner.GetRelic<WeiLaiShenRelic>();
        if (relic is null)
        {
            await RelicCmd.Obtain<WeiLaiShenRelic>(Owner);
        }
        else
        {
            relic.ResetDuration();
        }
    }

    private void UpgradeCombatCards()
    {
        var cards = Owner.PlayerCombatState?.AllCards
            .Where(card => card.Owner == Owner && card.IsUpgradable)
            .ToList();
        if (cards is { Count: > 0 })
        {
            CardCmd.Upgrade(cards, CardPreviewStyle.None);
        }
    }
}

[RegisterModelCapability]
[RegisterDefaultModelCapability(typeof(WeiLaiShen))]
public sealed class WeiLaiShenPlayResultCapability
    : CardCapability, ICardPlayResultContributor
{
    public PileType? GetResultPileTypeForCardPlay(CardModel card) =>
        PileType.None;
}
