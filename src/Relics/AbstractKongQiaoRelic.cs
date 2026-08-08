using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.Cards;

namespace GuZhenRen.Relics;

public abstract class AbstractKongQiaoRelic : ModRelicTemplate
{
    private bool _effectUsedThisCombat;
    private int _xp;

    public abstract int Rank { get; }

    protected abstract int NeededXp { get; }

    protected abstract string RelicImageName { get; }

    protected abstract RelicModel? NextStage { get; }

    public override RelicRarity Rarity =>
        Rank == 1 ? RelicRarity.Starter : RelicRarity.Event;

    [SavedProperty]
    public int Xp
    {
        get => _xp;
        set
        {
            AssertMutable();
            _xp = Math.Max(0, value);
        }
    }

    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"res://GuZhenRen/images/relics/{RelicImageName}.png",
        IconOutlinePath: $"res://GuZhenRen/images/relics/outline/{RelicImageName}.png",
        BigIconPath: $"res://GuZhenRen/images/relics/{RelicImageName}.png");

    public override Task BeforeCombatStart()
    {
        _effectUsedThisCombat = false;
        return Task.CompletedTask;
    }

    public override Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (!_effectUsedThisCombat &&
            !cardPlay.IsAutoPlay &&
            QualifiesForFreePlay(cardPlay.Card))
        {
            _effectUsedThisCombat = true;
            Flash();
        }

        return Task.CompletedTask;
    }

    public override bool TryModifyEnergyCostInCombat(
        CardModel card,
        decimal originalCost,
        out decimal modifiedCost)
    {
        modifiedCost = originalCost;

        if (_effectUsedThisCombat || !QualifiesForFreePlay(card))
        {
            return false;
        }

        modifiedCost = 0;
        return true;
    }

    public override async Task AfterCombatVictory(CombatRoom room)
    {
        if (NextStage == null)
        {
            return;
        }

        Xp += GetXpReward(room.RoomType);
        if (Xp < NeededXp)
        {
            return;
        }

        var nextStage = NextStage.ToMutable();
        if (nextStage is not AbstractKongQiaoRelic nextKongQiao)
        {
            throw new InvalidOperationException(
                $"{GetType().Name} next stage must inherit AbstractKongQiaoRelic.");
        }

        nextKongQiao.Xp = Xp - NeededXp;
        await RelicCmd.Replace(this, nextKongQiao);
    }

    private bool QualifiesForFreePlay(CardModel card)
    {
        return Rank > 1 &&
               card.Owner == Owner &&
               card is GuZhenRenCardTemplate guCard &&
               guCard.Rank >= 1 &&
               guCard.Rank < Rank;
    }

    private static int GetXpReward(RoomType roomType)
    {
        return roomType switch
        {
            RoomType.Monster => 1,
            RoomType.Elite => 3,
            RoomType.Boss => 5,
            _ => 0
        };
    }
}
