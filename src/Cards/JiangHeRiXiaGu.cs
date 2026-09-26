using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
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
    public override int Rank => IsUpgraded ? 5 : 4;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/JiangHeRiXiaGu.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.GuangDao];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(4, ValueProp.Move),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CombatHitsPreviewVar(),
        new CalculatedVar("CalculatedHits")
            .WithMultiplier(static (CardModel card, Creature? _) =>
                CalculateHits(card))
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

        var hits = CalculateHits(this);
        for (var i = 0; i < hits; i++)
        {
            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this, cardPlay)
                .TargetingAllOpponents(CombatState)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
    }

    private static int CalculateHits(CardModel card)
    {
        if (card.CombatState is null)
        {
            return 1;
        }

        var otherLightDaoCards = PileType.Hand.GetPile(card.Owner).Cards.Count(
            handCard => !ReferenceEquals(handCard, card)
                && GuZhenRenTagRules.HasEffectiveTag(
                    handCard,
                    GuZhenRenTags.GuangDao));
        return 1 + otherLightDaoCards;
    }

    private sealed class CombatHitsPreviewVar()
        : StringVar("CombatPreview")
    {
        public override void UpdateCardPreview(
            CardModel card,
            CardPreviewMode previewMode,
            Creature? target,
            bool runGlobalHooks)
        {
            if (card.CombatState is null)
            {
                StringValue = string.Empty;
                return;
            }

            var preview = new LocString(
                "cards",
                "GU_ZHEN_REN_CARD_JIANG_HE_RI_XIA_GU.combatPreview");
            preview.Add("Hits", CalculateHits(card));
            StringValue = preview.GetFormattedText();
        }
    }
}
