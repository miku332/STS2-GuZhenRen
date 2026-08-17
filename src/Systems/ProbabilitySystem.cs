using GuZhenRen.Powers;
using GuZhenRen.Relics;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace GuZhenRen.Systems;

public static class ProbabilitySystem
{
    private const decimal HongYunQiTianBonus = 40m;

    public static bool Roll(CardModel card, decimal chancePercent)
    {
        ArgumentNullException.ThrowIfNull(card.Owner);

        var chance = GetEffectiveChance(card, chancePercent);

        var success = card.Owner.RunState.Rng.CombatCardSelection.NextFloat(100f)
            < (float)chance;
        if (!success)
        {
            card.Owner.Creature.GetPower<ZhuanYunPower>()?.OnProbabilityRollFailed(card);
            RefreshCardVisuals(card);
        }

        return success;
    }

    public static decimal GetEffectiveChance(
        CardModel card,
        decimal baseChance)
    {
        if (card.Owner is null)
        {
            return Math.Clamp(baseChance, 0m, 100m);
        }

        if (card is GuZhenRen.Cards.AbstractXuYingCard
            && card.Owner.Creature.GetPower<QuanLiYiFuPower>() is not null)
        {
            return 100m;
        }

        var chance = baseChance;
        if (card.Owner.GetRelic<HongYunQiTianGu>() is not null)
        {
            chance += HongYunQiTianBonus;
        }

        if (card.Owner.Creature.GetPower<YunDaoDaoHenPower>() is { } yunDao)
        {
            chance += yunDao.ProbabilityBonus;
        }

        return Math.Clamp(chance, 0m, 100m);
    }

    public static void IncreaseHandProbabilities(
        Player player,
        decimal percentagePoints)
    {
        foreach (var card in PileType.Hand
                     .GetPile(player)
                     .Cards
                     .OfType<IProbabilityCard>()
                     .ToList())
        {
            card.IncreaseBaseChance(percentagePoints);

            if (card is CardModel model)
            {
                RefreshCardVisuals(model);
            }
        }
    }

    public static void DecreaseCombatProbabilities(
        MegaCrit.Sts2.Core.Entities.Players.Player player,
        decimal percentagePoints)
    {
        foreach (var card in CardPile.GetCards(
                     player,
                     PileType.Hand,
                     PileType.Draw,
                     PileType.Discard,
                     PileType.Exhaust,
                     PileType.Play)
                 .OfType<IProbabilityCard>()
                 .ToList())
        {
            card.IncreaseBaseChance(-percentagePoints);

            if (card is CardModel model)
            {
                RefreshCardVisuals(model);
            }
        }
    }

    private static void RefreshCardVisuals(CardModel card)
    {
        var node = NCard.FindOnTable(card);
        if (node is null)
        {
            return;
        }

        node.UpdateVisuals(
            card.Pile?.Type ?? PileType.Hand,
            CardPreviewMode.Normal);
    }
}

public sealed class ProbabilityVar : DynamicVar
{
    public ProbabilityVar(string name, decimal baseValue)
        : base(name, baseValue)
    {
    }

    public override void UpdateCardPreview(
        CardModel card,
        CardPreviewMode previewMode,
        Creature? target,
        bool runGlobalHooks)
    {
        PreviewValue = ProbabilitySystem.GetEffectiveChance(card, BaseValue);
    }
}
