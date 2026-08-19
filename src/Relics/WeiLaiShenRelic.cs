using GuZhenRen.CardPools;
using GuZhenRen.Cards;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interactions.RightClick;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class WeiLaiShenRelic
    : ModRelicTemplate, IModRightClickableRelic
{
    private int _counter = WeiLaiShen.Duration;
    private bool _isReturning;
    private bool _borrowedThisCombat;

    public override RelicRarity Rarity => RelicRarity.Event;

    public override bool ShowCounter => true;

    public override int DisplayAmount => Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Battles", WeiLaiShen.Duration)
    ];

    [SavedProperty]
    public int Counter
    {
        get => _counter;
        set
        {
            AssertMutable();
            _counter = Math.Clamp(value, 0, WeiLaiShen.Duration);
            DynamicVars["Battles"].BaseValue = _counter;
            InvokeDisplayAmountChanged();
        }
    }

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/relics/WeiLaiShenRelic.png",
        IconOutlinePath:
            "res://GuZhenRen/images/relics/outline/WeiLaiShenRelic.png",
        BigIconPath: "res://GuZhenRen/images/relics/WeiLaiShenRelic.png");

    public override Task AfterObtained()
    {
        Counter = WeiLaiShen.Duration;
        _borrowedThisCombat = false;
        return Task.CompletedTask;
    }

    public override Task BeforeCombatStart()
    {
        _borrowedThisCombat = false;
        if (Counter <= 0)
        {
            return Task.CompletedTask;
        }

        var cards = Owner.PlayerCombatState?.AllCards
            .Where(card => card.Owner == Owner && card.IsUpgradable)
            .ToList();
        if (cards is { Count: > 0 })
        {
            Flash();
            CardCmd.Upgrade(cards, CardPreviewStyle.None);
        }

        return Task.CompletedTask;
    }

    public override async Task AfterCombatVictory(CombatRoom room)
    {
        if (Counter <= 0 || _isReturning)
        {
            return;
        }

        Counter--;
        if (Counter <= 0)
        {
            await ReturnToCard(addCombatCopy: false);
        }
    }

    public bool CanHandleRightClickLocal(ModRightClickContext context) =>
        context.Player == Owner && !_isReturning;

    public async Task OnRightClick(ModRightClickExecutionContext context)
    {
        await ReturnToCard(
            addCombatCopy: CombatManager.Instance.IsInProgress);
    }

    public void ResetDuration()
    {
        Counter = WeiLaiShen.Duration;
        _borrowedThisCombat = false;
        Flash();
    }

    internal bool TryUseRecipeBorrow()
    {
        if (_borrowedThisCombat || Counter <= 0 || _isReturning)
        {
            return false;
        }

        _borrowedThisCombat = true;
        Flash();
        return true;
    }

    private async Task ReturnToCard(bool addCombatCopy)
    {
        if (_isReturning || HasBeenRemovedFromState)
        {
            return;
        }

        _isReturning = true;
        try
        {
            var deckCard = Owner.RunState.CreateCard<WeiLaiShen>(Owner);
            var result = await CardPileCmd.Add(
                deckCard,
                PileType.Deck,
                CardPilePosition.Bottom,
                null,
                false);
            if (!result.success)
            {
                return;
            }

            if (addCombatCopy && Owner.Creature.CombatState is not null)
            {
                var combatCard = Owner.Creature.CombatState
                    .CreateCard<WeiLaiShen>(Owner);
                combatCard.DeckVersion = result.cardAdded;
                await CardPileCmd.AddGeneratedCardToCombat(
                    combatCard,
                    PileType.Hand,
                    Owner,
                    CardPilePosition.Bottom);
            }

            Flash();
            await RelicCmd.Remove(this);
        }
        finally
        {
            _isReturning = false;
        }
    }
}
