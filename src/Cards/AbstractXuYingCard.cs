using System.Runtime.CompilerServices;
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
    private static readonly ConditionalWeakTable<CardPlay, object> ProcessedCardPlays = new();
    private static readonly object ProcessedCardPlayMarker = new();
    private static readonly object ProcessedCardPlayLock = new();

    internal readonly record struct XuYingTrigger(
        AbstractXuYingCard Card,
        CardPlay CardPlay);

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
            || cardPlay.Card.Tags.Contains(GuZhenRenTags.XuYing))
        {
            return;
        }

        lock (ProcessedCardPlayLock)
        {
            if (ProcessedCardPlays.TryGetValue(cardPlay, out _))
            {
                return;
            }

            ProcessedCardPlays.Add(cardPlay, ProcessedCardPlayMarker);
        }

        var triggers = PileType.Hand.GetPile(Owner)
            .Cards
            .OfType<AbstractXuYingCard>()
            .Where(shadow => ProbabilitySystem.Roll(
                shadow,
                shadow.DynamicVars["Chance"].BaseValue))
            .Select(shadow => new XuYingTrigger(shadow, cardPlay))
            .ToList();

        await TriggerBatch(choiceContext, triggers, this);
    }

    public async Task TriggerFromLiQiPower(
        PlayerChoiceContext choiceContext,
        Creature target)
    {
        if (Pile?.Type != PileType.Hand)
        {
            return;
        }

        await TriggerBatch(
            choiceContext,
            [new XuYingTrigger(this, CreateAutoPlay(target))]);
    }

    internal static CardPlay CreateAutoPlay(
        AbstractXuYingCard card,
        Creature target) =>
        new()
        {
            Card = card,
            Player = card.Owner,
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

    private CardPlay CreateAutoPlay(Creature target) =>
        CreateAutoPlay(this, target);

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

    internal static async Task TriggerBatch(
        PlayerChoiceContext choiceContext,
        IReadOnlyList<XuYingTrigger> triggers,
        AbstractXuYingCard? activeHookModel = null)
    {
        if (triggers.Count == 0)
        {
            return;
        }

        var previews = CreateTriggerPreviews(triggers);
        NestedXuYingEffectDepth.Value++;
        try
        {
            if (previews.Count > 0)
            {
                await Cmd.Wait(0.25f);
            }

            for (var i = 0; i < triggers.Count; i++)
            {
                if (CombatManager.Instance.IsOverOrEnding)
                {
                    break;
                }

                var trigger = triggers[i];
                var cardPlay = trigger.Card.ResolveTarget(trigger.CardPlay);
                if (cardPlay is null)
                {
                    FadeAndReleasePreview(FindPreview(previews, i));
                    continue;
                }

                var preview = FindPreview(previews, i);
                if (preview is not null)
                {
                    MovePreviewToCenter(preview);
                    await Cmd.Wait(0.18f);
                }

                var pushedModel = trigger.Card != activeHookModel;
                if (pushedModel)
                {
                    choiceContext.PushModel(trigger.Card);
                }

                try
                {
                    await trigger.Card.TriggerXuYingEffect(choiceContext, cardPlay);
                }
                finally
                {
                    if (pushedModel)
                    {
                        choiceContext.PopModel(trigger.Card);
                    }
                }

                if (preview is not null)
                {
                    await Cmd.Wait(0.18f);
                    FadeAndReleasePreview(preview);
                    await Cmd.Wait(0.12f);
                }
            }
        }
        finally
        {
            NestedXuYingEffectDepth.Value--;
            foreach (var preview in previews)
            {
                if (GodotObject.IsInstanceValid(preview))
                {
                    preview.QueueFreeSafely();
                }
            }
        }
    }

    private CardPlay? ResolveTarget(CardPlay cardPlay)
    {
        if (!RequiresLiveTarget
            || cardPlay.Target is { IsAlive: true })
        {
            return cardPlay;
        }

        var enemies = CombatState?.HittableEnemies
            .Where(enemy => enemy.IsAlive)
            .ToList();
        if (enemies is not { Count: > 0 })
        {
            return null;
        }

        return CopyWithTarget(
            cardPlay,
            Owner.RunState.Rng.CombatTargets.NextItem(enemies)!);
    }

    private static CardPlay CopyWithTarget(
        CardPlay source,
        Creature target) =>
        new()
        {
            Card = source.Card,
            Player = source.Player,
            Target = target,
            ResultPile = source.ResultPile,
            Resources = source.Resources,
            IsAutoPlay = source.IsAutoPlay,
            PlayIndex = source.PlayIndex,
            PlayCount = source.PlayCount
        };

    private static List<NCard?> CreateTriggerPreviews(
        IReadOnlyList<XuYingTrigger> triggers)
    {
        var previews = Enumerable.Repeat<NCard?>(null, triggers.Count).ToList();
        if (CombatManager.Instance.IsEnding
            || NCombatRoom.Instance?.Ui.CardPreviewContainer is not { } container)
        {
            return previews;
        }

        for (var i = 0; i < triggers.Count; i++)
        {
            var card = triggers[i].Card;
            if (!LocalContext.IsMine(card)
                || NCard.Create(card) is not { } preview)
            {
                continue;
            }

            container.AddChildSafely(preview);
            preview.UpdateVisuals(PileType.Hand, CardPreviewMode.Normal);
            preview.MouseFilter = Control.MouseFilterEnum.Ignore;
            preview.FocusMode = Control.FocusModeEnum.None;
            preview.ZIndex = 100;
            preview.PivotOffset = NCard.defaultSize / 2f;
            preview.Modulate = new Color(1f, 1f, 1f, 0f);
            previews[i] = preview;
        }

        var viewportSize = container.Size;
        if (viewportSize.X <= 0f || viewportSize.Y <= 0f)
        {
            viewportSize = container.GetViewportRect().Size;
        }

        var center = viewportSize / 2f + Vector2.Down * 25f;
        var horizontalRadius = MathF.Min(400f, viewportSize.X * 0.28f);
        var verticalRadius = MathF.Min(250f, viewportSize.Y * 0.23f);
        foreach (var preview in previews.OfType<NCard>())
        {
            var offset = new Vector2(
                Random.Shared.NextSingle() * horizontalRadius * 2f - horizontalRadius,
                Random.Shared.NextSingle() * verticalRadius * 2f - verticalRadius);
            preview.Position = center + offset;
            preview.Scale = Vector2.One * 0.55f;
            preview.Rotation = Random.Shared.NextSingle() * 0.12f - 0.06f;

            var tween = preview.CreateTween().SetParallel();
            tween.TweenProperty(preview, "scale", Vector2.One * 0.71f, 0.2f)
                .SetEase(Tween.EaseType.Out)
                .SetTrans(Tween.TransitionType.Cubic);
            tween.TweenProperty(preview, "modulate:a", 1f, 0.15f)
                .SetEase(Tween.EaseType.Out);
        }

        return previews;
    }

    private static NCard? FindPreview(IReadOnlyList<NCard?> previews, int index) =>
        index >= 0 && index < previews.Count
            ? previews[index]
            : null;

    private static void MovePreviewToCenter(NCard preview)
    {
        if (!GodotObject.IsInstanceValid(preview)
            || preview.GetParent() is not Control container)
        {
            return;
        }

        var viewportSize = container.Size;
        if (viewportSize.X <= 0f || viewportSize.Y <= 0f)
        {
            viewportSize = container.GetViewportRect().Size;
        }

        var center = viewportSize / 2f + Vector2.Down * 25f;
        var tween = preview.CreateTween();
        tween.SetParallel();
        tween.TweenProperty(preview, "position", center, 0.16f)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Cubic);
        tween.TweenProperty(preview, "scale", Vector2.One * 0.9f, 0.16f)
            .SetEase(Tween.EaseType.Out)
            .SetTrans(Tween.TransitionType.Cubic);
        tween.TweenProperty(preview, "rotation", 0f, 0.16f)
            .SetEase(Tween.EaseType.Out);
    }

    private static void FadeAndReleasePreview(NCard? preview)
    {
        if (preview is null || !GodotObject.IsInstanceValid(preview))
        {
            return;
        }

        var tween = preview.CreateTween().SetParallel();
        tween.TweenProperty(preview, "modulate:a", 0f, 0.12f)
            .SetEase(Tween.EaseType.In);
        tween.TweenProperty(preview, "scale", Vector2.One * 0.75f, 0.12f)
            .SetEase(Tween.EaseType.In);
        tween.Chain().TweenCallback(Callable.From(preview.QueueFreeSafely));
    }
}
