using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models.Capabilities;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Tags;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class JianQiaoGu : GuZhenRenCardTemplate
{
    public override int Rank => 4;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/JianQiaoGu.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.JianDao];

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(7, ValueProp.Move)
    ];

    public JianQiaoGu()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(
            Owner.Creature,
            DynamicVars.Block.BaseValue,
            ValueProp.Move,
            cardPlay,
            false);

        var selectorPrefs = new CardSelectorPrefs(SelectionScreenPrompt, 1);
        var selectedCards = await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            selectorPrefs,
            static card => card.Type == CardType.Attack
                && !card.Tags.Contains(GuZhenRenTags.XuYing)
                && !card.TryGetCapability<JianQiaoGuModifierCapability>(out _),
            this);
        var selected = selectedCards.FirstOrDefault();

        if (selected is null)
        {
            return;
        }

        selected.GetOrCreateCapability<JianQiaoGuModifierCapability>();
        await CardPileCmd.Add(
            selected,
            PileType.Draw,
            CardPilePosition.Bottom,
            null,
            false);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
    }
}

[RegisterModelCapability]
public sealed class JianQiaoGuModifierCapability : OneShotCardPlayCapability,
    ICardEnergyCostContributor
{
    protected override Task OnOwnerCardPlayedOnce(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay) =>
        Task.CompletedTask;

    public int ModifyEnergyCost(
        CardModel card,
        int originalCost,
        CostModifiers modifiers) =>
        modifiers.HasFlag(CostModifiers.Local) ? 0 : originalCost;

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay) =>
        dealer == Owner?.Owner.Creature
            && cardSource == Owner
            && props.IsPoweredAttack()
            ? 2m
            : 1m;
}
