using GuZhenRen.CardPools;
using GuZhenRen.Keywords;
using GuZhenRen.Systems;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class EYun : GuZhenRenCardTemplate
{
    private const int InitialCombatsToRemove = 3;
    private int _combatsRemaining = InitialCombatsToRemove;

    public override int Rank => 0;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/EYun.png");

    public override CardPoolModel VisualCardPool => ModelDb.CardPool<CurseCardPool>();

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        GuZhenRenKeywords.GaiLv,
        CardKeyword.Unplayable
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("CombatsRemaining", _combatsRemaining)
    ];

    public override bool CanBeGeneratedInCombat => false;

    public override bool CanBeGeneratedByModifiers => false;

    public EYun()
        : base(-1, CardType.Curse, CardRarity.Curse, TargetType.None, false)
    {
    }

    public void OnCardDrawn()
    {
        ProbabilitySystem.DecreaseCombatProbabilities(Owner, 10);
    }

    public async Task OnCombatEnded()
    {
        var deckVersion = DeckVersion as EYun;
        if (deckVersion is null && Pile?.Type == PileType.Deck)
        {
            deckVersion = this;
        }

        if (deckVersion is null)
        {
            return;
        }

        var remaining = Math.Max(0, deckVersion.CombatsRemaining - 1);
        deckVersion.SetCombatsRemaining(remaining);
        if (remaining == 0 && deckVersion.Pile?.Type == PileType.Deck)
        {
            await CardPileCmd.RemoveFromDeck(deckVersion, showPreview: false);
        }
    }

    protected override Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay) =>
        Task.CompletedTask;

    protected override void OnUpgrade()
    {
    }

    [SavedProperty]
    public int CombatsRemaining
    {
        get => _combatsRemaining;
        set
        {
            AssertMutable();
            _combatsRemaining = Math.Clamp(value, 0, InitialCombatsToRemove);
        }
    }

    private void SetCombatsRemaining(int value)
    {
        CombatsRemaining = value;
        DynamicVars["CombatsRemaining"].BaseValue = value;
    }
}
