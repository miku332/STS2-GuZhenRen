using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Powers;
using GuZhenRen.Tags;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class YuHuo : GuZhenRenCardTemplate
{
    private static ICombatState? s_trackedCombatState;
    private static int s_trackedTurnNumber = -1;
    private static readonly HashSet<CardModel> s_exhaustedCardsThisTurn = [];

    public override int Rank => IsUpgraded ? 7 : 6;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/YuHuo.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.YanDao];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<FenShaoPower>(1),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("ExhaustedThisTurn")
            .WithMultiplier(static (card, _) =>
                ((YuHuo)card).CountExhaustedThisTurn())
    ];

    public YuHuo()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(CombatState);

        var count = CountExhaustedThisTurn();
        if (count <= 0)
        {
            return;
        }

        for (var i = 0; i < count; i++)
        {
            foreach (var enemy in CombatState.HittableEnemies.ToList())
            {
                await PowerCmd.Apply<FenShaoPower>(
                    choiceContext,
                    enemy,
                    DynamicVars["FenShaoPower"].BaseValue,
                    Owner.Creature,
                    this);
            }
        }
    }

    public override Task AfterCardExhausted(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool causedByEthereal)
    {
        ResetTrackerIfNeeded();

        if (card.Owner == Owner)
        {
            s_exhaustedCardsThisTurn.Add(card);
        }

        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }

    private int CountExhaustedThisTurn()
    {
        ResetTrackerIfNeeded();
        return s_exhaustedCardsThisTurn.Count;
    }

    private void ResetTrackerIfNeeded()
    {
        var combatState = CombatState;
        var turnNumber = Owner.PlayerCombatState?.TurnNumber ?? -1;
        if (!ReferenceEquals(s_trackedCombatState, combatState)
            || s_trackedTurnNumber != turnNumber)
        {
            s_trackedCombatState = combatState;
            s_trackedTurnNumber = turnNumber;
            s_exhaustedCardsThisTurn.Clear();
        }
    }
}
