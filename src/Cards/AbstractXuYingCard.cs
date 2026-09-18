using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.Keywords;
using GuZhenRen.Powers;
using GuZhenRen.Systems;
using GuZhenRen.Tags;

namespace GuZhenRen.Cards;

public abstract class AbstractXuYingCard : GuZhenRenCardTemplate, IProbabilityCard
{
    private static readonly AsyncLocal<int> NestedXuYingEffectDepth = new();

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
            || NestedXuYingEffectDepth.Value > 0
            || cardPlay.Card == this
            || cardPlay.Card.Owner != Owner
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
                Player = Owner,
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
        NestedXuYingEffectDepth.Value++;
        var triggerCardNode = HideTriggerAttackCard(triggerCardPlay);
        var preview = ShowTriggerPreview();
        try
        {
            if (preview is not null)
            {
                await Cmd.Wait(0.2f);
            }

            await TriggerXuYingEffect(choiceContext, triggerCardPlay);

            if (preview is not null && GodotObject.IsInstanceValid(preview))
            {
                await Cmd.Wait(0.2f);
                FadeTriggerPreview(preview);
                await Cmd.Wait(0.2f);
            }
        }
        finally
        {
            if (preview is not null
                && GodotObject.IsInstanceValid(preview)
                && !preview.IsQueuedForDeletion())
            {
                preview.QueueFreeSafely();
            }

            if (triggerCardNode is not null
                && GodotObject.IsInstanceValid(triggerCardNode))
            {
                triggerCardNode.Visible = true;
            }

            NestedXuYingEffectDepth.Value--;
        }
    }

    private static NCard? HideTriggerAttackCard(CardPlay triggerCardPlay)
    {
        var triggerCard = triggerCardPlay.Card;
        if (triggerCard.Type != CardType.Attack
            || triggerCard.Tags.Contains(GuZhenRenTags.XuYing)
            || !LocalContext.IsMine(triggerCard)
            || NCard.FindOnTable(triggerCard) is not { } triggerCardNode)
        {
            return null;
        }

        triggerCardNode.Visible = false;
        return triggerCardNode;
    }

    private NCard? ShowTriggerPreview()
    {
        if (CombatManager.Instance.IsEnding
            || !LocalContext.IsMine(this)
            || NCombatRoom.Instance?.Ui.CardPreviewContainer is not { } container
            || NCard.Create(this) is not { } preview)
        {
            return null;
        }

        container.AddChildSafely(preview);
        preview.UpdateVisuals(PileType.Hand, CardPreviewMode.Normal);
        preview.Modulate = Colors.White;
        preview.MouseFilter = Control.MouseFilterEnum.Ignore;
        preview.FocusMode = Control.FocusModeEnum.None;

        var tween = preview.CreateTween();
        tween.TweenProperty(preview, "scale", Vector2.One, 0.2f)
            .From(Vector2.Zero)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Cubic);

        return preview;
    }

    private static void FadeTriggerPreview(NCard preview)
    {
        var tween = preview.CreateTween();
        tween.TweenProperty(preview, "modulate:a", 0f, 0.2f)
            .SetEase(Tween.EaseType.In);
    }
}
