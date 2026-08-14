using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Tags;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class XueShouYinGu : GuZhenRenCardTemplate
{
    public override int Rank => 4;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/XueShouYinGu.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.XueDao];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(7, ValueProp.Move)
    ];

    public XueShouYinGu()
        : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);

        var hand = PileType.Hand.GetPile(Owner);
        if (hand.Cards.Count == 0)
        {
            return;
        }

        var selectedCards = await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 1),
            static _ => true,
            this);
        var selected = selectedCards.FirstOrDefault();
        if (selected is null)
        {
            return;
        }

        if (selected.Type == CardType.Attack)
        {
            var target = GetFollowUpTarget(cardPlay.Target);
            if (target is null)
            {
                return;
            }

            if (selected.EnergyCost.CostsX)
            {
                selected.EnergyCost.CapturedXValue = Owner.PlayerCombatState?.Energy ?? 0;
            }

            selected.SetToFreeThisTurn();
            selected.ExhaustOnNextPlay = true;
            await CardCmd.AutoPlay(
                choiceContext,
                selected,
                target,
                AutoPlayType.Default,
                selected.EnergyCost.CostsX,
                false);
            return;
        }

        await CardCmd.Exhaust(choiceContext, selected);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }

    private Creature? GetFollowUpTarget(Creature? originalTarget)
    {
        if (originalTarget?.IsAlive == true)
        {
            return originalTarget;
        }

        var aliveEnemies = CombatState?.HittableEnemies
            .Where(enemy => enemy.IsAlive)
            .ToList();
        return aliveEnemies is { Count: > 0 }
            ? Owner.RunState.Rng.CombatTargets.NextItem(aliveEnemies)
            : null;
    }
}
