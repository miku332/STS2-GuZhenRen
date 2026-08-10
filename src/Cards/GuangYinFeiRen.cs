using GuZhenRen.CardPools;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class GuangYinFeiRen : AbstractShaZhaoCard
{
    private const int InitialUses = 3;

    private int _remainingUses = InitialUses;

    public override int Rank => 0;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/GuangYinFeiRen.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.ZhouDao];

    [SavedProperty]
    public int RemainingUses
    {
        get => _remainingUses;
        set
        {
            AssertMutable();
            _remainingUses = Math.Clamp(value, 0, InitialUses);
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("RemainingUses", RemainingUses)
    ];

    public GuangYinFeiRen()
        : base(4, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy, false)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await CreatureCmd.Kill(cardPlay.Target);
        await ConsumeUse();
    }

    protected override PileType GetResultPileTypeForCardPlay()
    {
        return RemainingUses <= 1
            ? PileType.None
            : base.GetResultPileTypeForCardPlay();
    }

    private async Task ConsumeUse()
    {
        var remainingUses = Math.Max(0, RemainingUses - 1);
        SetRemainingUses(this, remainingUses);

        if (DeckVersion is not GuangYinFeiRen deckVersion)
        {
            return;
        }

        SetRemainingUses(deckVersion, remainingUses);
        if (remainingUses == 0 && deckVersion.Pile?.Type == PileType.Deck)
        {
            await CardPileCmd.RemoveFromDeck(deckVersion, showPreview: false);
        }
    }

    private static void SetRemainingUses(
        GuangYinFeiRen card,
        int remainingUses)
    {
        card.RemainingUses = remainingUses;
        card.DynamicVars["RemainingUses"].BaseValue = remainingUses;
    }
}
