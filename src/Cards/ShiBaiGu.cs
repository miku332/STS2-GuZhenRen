using GuZhenRen.CardPools;
using GuZhenRen.Tags;
using GuZhenRen.Systems;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class ShiBaiGu : GuZhenRenCardTemplate, IProbabilityCard
{
    public override int Rank => IsUpgraded ? 2 : 1;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/ShiBaiGu.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.LuDao];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("HpLoss", 4),
        new DynamicVar("Chance", 1)
    ];

    public ShiBaiGu()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await CreatureCmd.Damage(
            choiceContext,
            Owner.Creature,
            DynamicVars["HpLoss"].BaseValue,
            ValueProp.Unblockable | ValueProp.Unpowered,
            Owner.Creature,
            this);

        if (Owner.Creature.IsDead)
        {
            return;
        }

        if (!ProbabilitySystem.Roll(this, DynamicVars["Chance"].BaseValue))
        {
            return;
        }

        ArgumentNullException.ThrowIfNull(CombatState);

        var success = CombatState.CreateCard<ChengGongGu>(Owner);
        await CardPileCmd.AddGeneratedCardToCombat(
            success,
            PileType.Hand,
            Owner,
            CardPilePosition.Bottom);

        if (DeckVersion is not null && DeckVersion.Pile?.Type == PileType.Deck)
        {
            await CardPileCmd.RemoveFromDeck(DeckVersion, showPreview: true);
        }

        await CardPileCmd.Add(
            this,
            PileType.Exhaust,
            CardPilePosition.Bottom,
            null,
            false);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["HpLoss"].UpgradeValueBy(-2);
    }

    public void IncreaseBaseChance(decimal percentagePoints)
    {
        var chance = DynamicVars["Chance"];
        chance.BaseValue = Math.Clamp(
            chance.BaseValue + percentagePoints,
            0m,
            100m);
    }
}
