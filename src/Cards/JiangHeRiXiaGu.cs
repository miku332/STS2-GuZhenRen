using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Tags;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class JiangHeRiXiaGu : GuZhenRenCardTemplate
{
    public override int Rank => 4;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/JiangHeRiXiaGu.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.GuangDao];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(4, ValueProp.Move),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("CalculatedHits")
            .WithMultiplier(static (CardModel card, Creature? _) =>
            {
                var hand = PileType.Hand.GetPile(card.Owner);
                return hand.Cards.Count(
                    handCard => GuZhenRenTagRules.HasEffectiveTag(
                        handCard,
                        GuZhenRenTags.GuangDao));
            })
    ];

    public JiangHeRiXiaGu()
        : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(CombatState);

        var handLightDaoCount = PileType.Hand.GetPile(Owner).Cards.Count(
            card => GuZhenRenTagRules.HasEffectiveTag(
                card,
                GuZhenRenTags.GuangDao));
        var hits = handLightDaoCount + (Pile?.Type == PileType.Hand ? 0 : 1);

        for (var i = 0; i < hits; i++)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .TargetingAllOpponents(CombatState)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
    }
}
