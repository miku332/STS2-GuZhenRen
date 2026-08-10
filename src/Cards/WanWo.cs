using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Keywords;
using GuZhenRen.Tags;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class WanWo : GuZhenRenCardTemplate
{
    protected override bool HasEnergyCostX => true;

    public override int Rank => 0;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/WanWo.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.LiDao];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        GuZhenRenKeywords.XuYing,
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<GeneratedCardPreview> GeneratedCardPreviews =>
        [PreviewCard<WoLiXuYing>(upgraded: true)];

    public WanWo()
        : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self, false)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(CombatState);

        var count = ResolveEnergyXValue();
        if (count <= 0)
        {
            return;
        }

        for (var i = 0; i < count; i++)
        {
            var shadow = CombatState.CreateCard<WoLiXuYing>(Owner);
            shadow.UpgradeInternal();
            shadow.FinalizeUpgradeInternal();

            await CardPileCmd.AddGeneratedCardToCombat(
                shadow,
                PileType.Hand,
                Owner,
                CardPilePosition.Bottom);

            var target = GetRandomLivingEnemy();
            if (target is not null)
            {
                await shadow.TriggerFromLiQiPower(choiceContext, target);
            }
        }
    }

    private Creature? GetRandomLivingEnemy()
    {
        var aliveEnemies = CombatState?.HittableEnemies
            .Where(enemy => enemy.IsAlive)
            .ToList();

        return aliveEnemies is { Count: > 0 }
            ? Owner.RunState.Rng.CombatTargets.NextItem(aliveEnemies)
            : null;
    }
}
