using GuZhenRen.CardPools;
using GuZhenRen.Cards;
using GuZhenRen.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interactions.RightClick;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class JianMianCengXiangShiRelic
    : ModRelicTemplate, IModRightClickableRelic
{
    private int _counter = JianMianCengXiangShi.Duration;
    private bool _isReturning;

    public override RelicRarity Rarity => RelicRarity.Event;

    public override bool ShowCounter => true;

    public override int DisplayAmount => Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Battles", JianMianCengXiangShi.Duration),
        new DynamicVar("FriendStacks", JianMianCengXiangShi.FriendStacks)
    ];

    [SavedProperty]
    public int Counter
    {
        get => _counter;
        set
        {
            AssertMutable();
            _counter = Math.Clamp(
                value,
                0,
                JianMianCengXiangShi.Duration);
            DynamicVars["Battles"].BaseValue = _counter;
            InvokeDisplayAmountChanged();
        }
    }

    public override RelicAssetProfile AssetProfile => new(
        IconPath:
            "res://GuZhenRen/images/relics/JianMianCengXiangShiRelic.png",
        IconOutlinePath:
            "res://GuZhenRen/images/relics/outline/JianMianCengXiangShiRelic.png",
        BigIconPath:
            "res://GuZhenRen/images/relics/JianMianCengXiangShiRelic.png");

    public override Task AfterObtained()
    {
        Counter = JianMianCengXiangShi.Duration;
        return Task.CompletedTask;
    }

    public override async Task BeforeCombatStart()
    {
        if (Counter <= 0 || Owner.Creature.CombatState is null)
        {
            return;
        }

        Flash();
        var choiceContext = new ThrowingPlayerChoiceContext();
        foreach (var enemy in Owner.Creature.CombatState.Enemies
                     .Where(static enemy => enemy.IsAlive)
                     .ToList())
        {
            await PowerCmd.Apply<HaoYouPower>(
                choiceContext,
                enemy,
                JianMianCengXiangShi.FriendStacks,
                Owner.Creature,
                null);
        }

        Counter--;
    }

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (player == Owner && Counter <= 0)
        {
            await ReturnToCard(addCombatCopy: true);
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
        Counter = JianMianCengXiangShi.Duration;
        Flash();
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
            var deckCard = Owner.RunState.CreateCard<JianMianCengXiangShi>(
                Owner);
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
                    .CreateCard<JianMianCengXiangShi>(Owner);
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
