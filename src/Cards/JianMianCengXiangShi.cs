using GuZhenRen.CardPools;
using GuZhenRen.Relics;
using GuZhenRen.Powers;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class JianMianCengXiangShi : AbstractShaZhaoCard
{
    public const int Duration = 5;
    public const int FriendStacks = 5;

    public override int Rank => 0;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/JianMianCengXiangShi.png");

    public override IEnumerable<CardTag> Tags =>
        [GuZhenRenTags.BianHuaDao];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Battles", Duration),
        new DynamicVar("FriendStacks", FriendStacks),
        new PowerVar<HaoYouPower>(0).WithPowerTooltip()
    ];

    public override bool CanBeGeneratedInCombat => false;

    public override bool CanBeGeneratedByModifiers => false;

    public JianMianCengXiangShi()
        : base(2, CardType.Skill, CardRarity.Token, TargetType.None, false)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        var deckCard = DeckVersion as JianMianCengXiangShi;
        if (deckCard is null && Pile?.Type == PileType.Deck)
        {
            deckCard = this;
        }

        deckCard ??= Owner.Deck.Cards
            .OfType<JianMianCengXiangShi>()
            .FirstOrDefault();

        if (deckCard?.Pile?.Type == PileType.Deck)
        {
            await CardPileCmd.RemoveFromDeck(deckCard, showPreview: false);
        }

        var relic = Owner.GetRelic<JianMianCengXiangShiRelic>();
        if (relic is null)
        {
            await RelicCmd.Obtain<JianMianCengXiangShiRelic>(Owner);
        }
        else
        {
            relic.ResetDuration();
        }
    }
}
