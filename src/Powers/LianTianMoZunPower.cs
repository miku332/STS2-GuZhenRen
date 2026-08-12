using GuZhenRen.Cards;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class LianTianMoZunPower : ModPowerTemplate
{
    private sealed class TriggerState
    {
        public bool TriggeredThisTurn;
    }

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/LianTianMoZunPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/LianTianMoZunPower_p.png");

    protected override object InitInternalData() => new TriggerState();

    public override async Task AfterCardExhausted(
        PlayerChoiceContext choiceContext,
        CardModel card,
        bool causedByEthereal)
    {
        var state = GetInternalData<TriggerState>();
        if (state.TriggeredThisTurn
            || card.Owner.Creature != Owner
            || card.Type == CardType.Status
            || !Owner.IsAlive
            || Owner.Player is null
            || Owner.CombatState is null)
        {
            return;
        }

        var sourceDaoTags = GuZhenRenTagRules
            .GetEffectiveDaoTags(card)
            .ToHashSet();
        if (sourceDaoTags.Count == 0)
        {
            return;
        }

        var options = CreateOptions(sourceDaoTags);
        if (options.Count == 0)
        {
            return;
        }

        state.TriggeredThisTurn = true;
        Flash();

        var selected = options.Count == 1
            ? options[0]
            : await CardSelectCmd.FromChooseACardScreen(
                choiceContext,
                options,
                Owner.Player);
        if (selected is null)
        {
            return;
        }

        await CardPileCmd.AddGeneratedCardToCombat(
            selected,
            PileType.Hand,
            Owner.Player,
            CardPilePosition.Bottom);
    }

    public override Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (side == CombatSide.Player && participants.Contains(Owner))
        {
            GetInternalData<TriggerState>().TriggeredThisTurn = false;
        }

        return Task.CompletedTask;
    }

    private List<CardModel> CreateOptions(HashSet<CardTag> sourceDaoTags)
    {
        var candidates = Owner.Player!.Character.CardPool
            .GetUnlockedCards(
                Owner.Player.UnlockState,
                Owner.Player.RunState.CardMultiplayerConstraint)
            .OfType<GuZhenRenCardTemplate>()
            .Where(IsOrdinaryGu)
            .Where(candidate => GuZhenRenTagRules
                .GetEffectiveDaoTags(candidate)
                .Any(sourceDaoTags.Contains))
            .Cast<CardModel>()
            .DistinctBy(candidate => candidate.Id)
            .ToList();

        var rng = Owner.Player.RunState.Rng.CombatCardGeneration;
        for (var i = candidates.Count - 1; i > 0; i--)
        {
            var swapIndex = rng.NextInt(i + 1);
            (candidates[i], candidates[swapIndex]) =
                (candidates[swapIndex], candidates[i]);
        }

        var options = new List<CardModel>();
        foreach (var canonical in candidates.Take(3))
        {
            var generated = Owner.CombatState!.CreateCard(canonical, Owner.Player!);
            generated.SetToFreeThisTurn();
            generated.AddKeyword(CardKeyword.Exhaust);
            generated.AddKeyword(CardKeyword.Ethereal);
            options.Add(generated);
        }

        return options;
    }

    private static bool IsOrdinaryGu(GuZhenRenCardTemplate card) =>
        card.Rank is >= 1 and <= 5
        && card.Rarity is CardRarity.Common or CardRarity.Uncommon or CardRarity.Rare
        && card.Type is CardType.Attack or CardType.Skill or CardType.Power
        && card is not AbstractBenMingGuCard
        && card is not AbstractShaZhaoCard
        && card is not AbstractXuYingCard
        && card.CanBeGeneratedInCombat;
}
