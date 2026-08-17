using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.Keywords;
using GuZhenRen.Powers;
using GuZhenRen.Systems;
using GuZhenRen.Tags;

namespace GuZhenRen.Cards;

public abstract class AbstractXuYingCard : GuZhenRenCardTemplate, IProbabilityCard
{
    private static int _nestedXuYingEffectDepth;

    protected abstract int ChancePercent { get; }

    protected virtual IEnumerable<DynamicVar> AdditionalVars => [];

    protected virtual bool RequiresLiveTarget => true;

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.LiDao, GuZhenRenTags.XuYing];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        GuZhenRenKeywords.GaiLv,
        CardKeyword.Retain,
        CardKeyword.Unplayable
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get
        {
            yield return new ProbabilityVar("Chance", ChancePercent);

            foreach (var dynamicVar in AdditionalVars)
            {
                yield return dynamicVar;
            }
        }
    }

    protected AbstractXuYingCard(CardType cardType, TargetType targetType)
        : base(-2, cardType, CardRarity.Basic, targetType, true)
    {
    }

    public override async Task AfterCardPlayedLate(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (Pile?.Type != PileType.Hand
            || _nestedXuYingEffectDepth > 0
            || cardPlay.Card == this
            || cardPlay.Card.Type != CardType.Attack
            || cardPlay.Card.Tags.Contains(GuZhenRenTags.XuYing)
            || (RequiresLiveTarget && (cardPlay.Target is null || !cardPlay.Target.IsAlive)))
        {
            return;
        }

        if (!ProbabilitySystem.Roll(
                this,
                DynamicVars["Chance"].BaseValue))
        {
            return;
        }

        await TriggerXuYingEffectWithRecursionGuard(choiceContext, cardPlay);
    }

    public async Task TriggerFromLiQiPower(
        PlayerChoiceContext choiceContext,
        Creature target)
    {
        if (Pile?.Type != PileType.Hand)
        {
            return;
        }

        await TriggerXuYingEffectWithRecursionGuard(
            choiceContext,
            new CardPlay
            {
                Card = this,
                Target = target,
                ResultPile = PileType.None,
                Resources = new ResourceInfo
                {
                    EnergySpent = 0,
                    EnergyValue = 0,
                    StarsSpent = 0,
                    StarValue = 0
                },
                IsAutoPlay = true,
                PlayIndex = 0,
                PlayCount = 1
            });
    }

    protected override Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay) =>
        Task.CompletedTask;

    protected abstract Task TriggerXuYingEffect(
        PlayerChoiceContext choiceContext,
        CardPlay triggerCardPlay);

    public void IncreaseBaseChance(decimal percentagePoints)
    {
        var chance = DynamicVars["Chance"];
        chance.BaseValue = Math.Clamp(
            chance.BaseValue + percentagePoints,
            0m,
            100m);
    }

    private async Task TriggerXuYingEffectWithRecursionGuard(
        PlayerChoiceContext choiceContext,
        CardPlay triggerCardPlay)
    {
        _nestedXuYingEffectDepth++;
        try
        {
            await TriggerXuYingEffect(choiceContext, triggerCardPlay);
        }
        finally
        {
            _nestedXuYingEffectDepth--;
        }
    }
}
