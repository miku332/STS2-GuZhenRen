using GuZhenRen.CardPools;
using GuZhenRen.Powers;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class LaiYinQuGuo : AbstractShaZhaoCard
{
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/LaiYinQuGuo.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.MuDao];

    public LaiYinQuGuo()
        : base(2, CardType.Skill, CardRarity.Token, TargetType.AllEnemies, false)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(CombatState);

        var fruits = Owner.Creature
            .GetPowerInstances<GuoPower>()
            .ToList();
        if (fruits.Count == 0)
        {
            return;
        }

        var enemies = CombatState.HittableEnemies
            .Where(static enemy => enemy.IsAlive)
            .ToList();
        if (enemies.Count == 0)
        {
            foreach (var fruit in fruits)
            {
                await PowerCmd.Remove(fruit);
            }

            return;
        }

        foreach (var fruit in fruits)
        {
            var target = Owner.RunState.Rng.CombatTargets.NextItem(enemies);
            if (target is null)
            {
                continue;
            }

            await PowerCmd.Remove(fruit);
            var copy = fruit.CreateTransferCopy();
            await PowerCmd.Apply(
                choiceContext,
                copy,
                target,
                fruit.Amount,
                Owner.Creature,
                this);
        }
    }
}
