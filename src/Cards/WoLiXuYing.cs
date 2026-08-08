using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Tags;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class WoLiXuYing : AbstractXuYingCard
{
    private CardModel? _selectedAttack;

    protected override int ChancePercent => 15;

    protected override bool RequiresLiveTarget => false;

    public override int Rank => 6;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/WoLiXuYing.png");

    public WoLiXuYing()
        : base(CardType.Attack, TargetType.None)
    {
    }

    protected override async Task TriggerXuYingEffect(
        PlayerChoiceContext choiceContext,
        CardPlay triggerCardPlay)
    {
        var sourceAttack = GetOrRollSelectedAttack();
        if (sourceAttack is null)
        {
            return;
        }

        var clone = sourceAttack.CreateClone();
        clone.ExhaustOnNextPlay = true;

        await CardPileCmd.AddGeneratedCardToCombat(
            clone,
            PileType.Hand,
            Owner,
            CardPilePosition.Bottom);

        var target = GetAutoPlayTarget(clone, triggerCardPlay.Target);
        if (clone.TargetType == TargetType.AnyEnemy && target is null)
        {
            await CardPileCmd.Add(
                clone,
                PileType.Exhaust,
                CardPilePosition.Top,
                null,
                false);
            RollSelectedAttack();
            return;
        }

        if (clone.EnergyCost.CostsX)
        {
            clone.EnergyCost.CapturedXValue = Owner.PlayerCombatState?.Energy ?? 0;
        }

        clone.SetToFreeThisTurn();
        await CardCmd.AutoPlay(
            choiceContext,
            clone,
            target,
            AutoPlayType.Default,
            clone.EnergyCost.CostsX,
            false);

        RollSelectedAttack();
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Chance"].UpgradeValueBy(10);
    }

    private CardModel? GetOrRollSelectedAttack()
    {
        if (_selectedAttack is { HasBeenRemovedFromState: false })
        {
            return _selectedAttack;
        }

        return RollSelectedAttack();
    }

    private CardModel? RollSelectedAttack()
    {
        var attacks = GetCandidateAttacks().ToList();
        _selectedAttack = attacks.Count == 0
            ? null
            : Owner.RunState.Rng.CombatCardSelection.NextItem(attacks);
        return _selectedAttack;
    }

    private IEnumerable<CardModel> GetCandidateAttacks()
    {
        var seen = new HashSet<CardModel>(ReferenceEqualityComparer.Instance);
        foreach (var pileType in new[]
                 {
                     PileType.Hand,
                     PileType.Draw,
                     PileType.Discard,
                     PileType.Exhaust,
                     PileType.Play
                 })
        {
            foreach (var card in pileType.GetPile(Owner).Cards)
            {
                var candidate = card.DeckVersion;
                if (candidate is null
                    || candidate.Type != CardType.Attack
                    || candidate.Tags.Contains(GuZhenRenTags.XuYing)
                    || candidate.Keywords.Contains(CardKeyword.Exhaust)
                    || candidate.Keywords.Contains(CardKeyword.Unplayable)
                    || !seen.Add(candidate))
                {
                    continue;
                }

                yield return card;
            }
        }
    }

    private Creature? GetAutoPlayTarget(CardModel card, Creature? preferredTarget)
    {
        if (card.TargetType != TargetType.AnyEnemy)
        {
            return null;
        }

        if (preferredTarget?.IsAlive == true)
        {
            return preferredTarget;
        }

        var aliveEnemies = CombatState?.HittableEnemies
            .Where(enemy => enemy.IsAlive)
            .ToList();
        return aliveEnemies is { Count: > 0 }
            ? Owner.RunState.Rng.CombatTargets.NextItem(aliveEnemies)
            : null;
    }
}
