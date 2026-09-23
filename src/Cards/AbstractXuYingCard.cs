using Godot;
using System.Runtime.CompilerServices;
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
using GuZhenRen.Patches;
using GuZhenRen.Powers;
using GuZhenRen.Systems;
using GuZhenRen.Tags;

namespace GuZhenRen.Cards;

public abstract class AbstractXuYingCard : GuZhenRenCardTemplate, IProbabilityCard
{
    private static readonly ConditionalWeakTable<CardPlay, ProcessedCardPlayMarker>
        ProcessedCardPlays = new();
    private static int _nestedXuYingEffectDepth;

    protected abstract int ChancePercent { get; }

    protected virtual IEnumerable<DynamicVar> AdditionalVars => [];

    protected virtual bool RequiresLiveTarget => true;

    protected virtual bool HidePreviewDuringEffect => false;

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
        if (_nestedXuYingEffectDepth > 0
            || cardPlay.Card.Owner != Owner
            || cardPlay.Card.Type != CardType.Attack
            || cardPlay.Card.Tags.Contains(GuZhenRenTags.XuYing)
            || cardPlay.Card == this)
        {
            return;
        }

        if (ProcessedCardPlays.TryGetValue(cardPlay, out _))
        {
            return;
        }

        ProcessedCardPlays.Add(cardPlay, new ProcessedCardPlayMarker());

        var triggeredShadows = PileType.Hand.GetPile(Owner)
            .Cards
            .OfType<AbstractXuYingCard>()
            .Where(shadow => shadow.CanTriggerFrom(cardPlay))
            .Where(shadow => ProbabilitySystem.Roll(
                shadow,
                shadow.DynamicVars["Chance"].BaseValue))
            .ToList();
        if (triggeredShadows.Count == 0)
        {
            return;
        }

        if (cardPlay.ResultPile != PileType.None)
        {
            XuYingTriggerCardVisualPatch.Mark(cardPlay.Card);
        }

        await TriggerBatch(
            choiceContext,
            triggeredShadows.Select(shadow => (shadow, cardPlay)));
    }

    public async Task TriggerFromLiQiPower(
        PlayerChoiceContext choiceContext,
        Creature target)
    {
        if (Pile?.Type != PileType.Hand)
        {
            return;
        }

        await TriggerBatchFromLiQiPower(
            choiceContext,
            [(this, target)]);
    }

    internal static Task TriggerBatchFromLiQiPower(
        PlayerChoiceContext choiceContext,
        IEnumerable<(AbstractXuYingCard Shadow, Creature Target)> triggers) =>
        TriggerBatch(
            choiceContext,
            triggers
                .Where(static trigger => trigger.Shadow.Pile?.Type == PileType.Hand)
                .Select(static trigger =>
                    (trigger.Shadow, trigger.Shadow.CreateAutoTriggerCardPlay(trigger.Target))));

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

    private bool CanTriggerFrom(CardPlay cardPlay) =>
        Pile?.Type == PileType.Hand
        && (!RequiresLiveTarget
            || (cardPlay.Target is not null && cardPlay.Target.IsAlive));

    private CardPlay CreateAutoTriggerCardPlay(Creature target) =>
        new()
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
        };

    private static async Task TriggerBatch(
        PlayerChoiceContext choiceContext,
        IEnumerable<(AbstractXuYingCard Shadow, CardPlay CardPlay)> triggers)
    {
        var pending = triggers
            .Select(trigger => new PendingTrigger(
                trigger.Shadow,
                trigger.CardPlay,
                trigger.Shadow.ShowTriggerPreview()))
            .ToList();
        if (pending.Count == 0)
        {
            return;
        }

        _nestedXuYingEffectDepth++;
        try
        {
            if (pending.Any(static trigger => trigger.Preview is not null))
            {
                await Cmd.Wait(0.2f);
            }

            foreach (var trigger in pending)
            {
                if (trigger.Preview is { } preview
                    && GodotObject.IsInstanceValid(preview))
                {
                    FocusTriggerPreview(preview);
                    await Cmd.Wait(0.2f);

                    if (trigger.Shadow.HidePreviewDuringEffect)
                    {
                        preview.Visible = false;
                    }
                }

                await trigger.Shadow.TriggerXuYingEffect(
                    choiceContext,
                    trigger.CardPlay);

                if (trigger.Preview is { } resolvedPreview
                    && GodotObject.IsInstanceValid(resolvedPreview))
                {
                    if (resolvedPreview.Visible)
                    {
                        await Cmd.Wait(0.2f);
                        FadeTriggerPreview(resolvedPreview);
                        await Cmd.Wait(0.2f);
                    }

                    ReleaseTriggerPreview(trigger);
                }
            }
        }
        finally
        {
            foreach (var trigger in pending)
            {
                ReleaseTriggerPreview(trigger);
            }

            XuYingTriggerCardVisualPatch.RestoreMissingHandCards(
                pending.Select(static trigger => trigger.Shadow));
            _nestedXuYingEffectDepth--;
        }
    }

    private NCard? ShowTriggerPreview()
    {
        if (CombatManager.Instance.IsEnding
            || !LocalContext.IsMine(this)
            || NCombatRoom.Instance?.Ui.MessyCardPreviewContainer is not { } container
            || NCard.Create(CreateClone()) is not { } preview)
        {
            return null;
        }

        container.AddChildSafely(preview);
        preview.UpdateVisuals(PileType.Hand, CardPreviewMode.Normal);
        preview.Modulate = Colors.White;
        preview.Rotation = 0f;
        preview.MouseFilter = Control.MouseFilterEnum.Ignore;
        preview.FocusMode = Control.FocusModeEnum.None;

        var tween = preview.CreateTween();
        tween.TweenProperty(preview, "scale", Vector2.One, 0.2f)
            .From(Vector2.Zero)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Cubic);

        return preview;
    }

    private static void FocusTriggerPreview(NCard preview)
    {
        if (preview.GetParent() is not Control container)
        {
            return;
        }

        preview.ZIndex = 1000;
        var tween = preview.CreateTween().SetParallel();
        tween.TweenProperty(preview, "position", container.Size * 0.5f, 0.2f)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Cubic);
        tween.TweenProperty(preview, "scale", Vector2.One, 0.2f)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Cubic);
    }

    private static void FadeTriggerPreview(NCard preview)
    {
        var tween = preview.CreateTween();
        tween.TweenProperty(preview, "modulate:a", 0f, 0.2f)
            .SetEase(Tween.EaseType.In);
    }

    private static void ReleaseTriggerPreview(PendingTrigger trigger)
    {
        if (trigger.PreviewReleased
            || trigger.Preview is not { } preview
            || !GodotObject.IsInstanceValid(preview))
        {
            return;
        }

        trigger.PreviewReleased = true;
        preview.Visible = true;
        preview.ZIndex = 0;
        preview.Modulate = Colors.White;
        preview.MouseFilter = Control.MouseFilterEnum.Stop;
        preview.FocusMode = Control.FocusModeEnum.None;
        preview.QueueFreeSafely();
    }

    private sealed class PendingTrigger(
        AbstractXuYingCard shadow,
        CardPlay cardPlay,
        NCard? preview)
    {
        public AbstractXuYingCard Shadow { get; } = shadow;

        public CardPlay CardPlay { get; } = cardPlay;

        public NCard? Preview { get; } = preview;

        public bool PreviewReleased { get; set; }
    }

    private sealed class ProcessedCardPlayMarker;
}
