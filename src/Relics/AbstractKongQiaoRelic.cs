using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using GuZhenRen.Powers;
using GuZhenRen.Systems;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.Cards;

namespace GuZhenRen.Relics;

public abstract class AbstractKongQiaoRelic : ModRelicTemplate
{
    private enum KongQiaoState
    {
        XpGathering,
        TribulationPending,
        Countdown
    }

    private const int BattlesPerTribulation = 2;
    private bool _effectUsedThisCombat;
    private int _xp;
    private KongQiaoState _state;
    private int _battlesToNextTribulation;

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

    [SavedProperty]
    public int TribulationState
    {
        get => (int)_state;
        set
        {
            AssertMutable();
            _state = Enum.IsDefined(typeof(KongQiaoState), value)
                ? (KongQiaoState)value
                : KongQiaoState.XpGathering;
        }
    }

    [SavedProperty]
    public int BattlesToNextTribulation
    {
        get => _battlesToNextTribulation;
        set
        {
            AssertMutable();
            _battlesToNextTribulation = Math.Max(0, value);
        }
    }

    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"res://GuZhenRen/images/relics/{RelicImageName}.png",
        IconOutlinePath: $"res://GuZhenRen/images/relics/outline/{RelicImageName}.png",
        BigIconPath: $"res://GuZhenRen/images/relics/{RelicImageName}.png");

    public override async Task BeforeCombatStart()
    {
        _effectUsedThisCombat = false;

        // Only the fifth rank breakthrough and later immortal ranks can trigger tribulation.
        // Reset stale saved state from older builds so ranks 1-4 never enter the trigger path.
        if (Rank < 5)
        {
            if (_state != KongQiaoState.XpGathering)
            {
                _state = KongQiaoState.XpGathering;
                _battlesToNextTribulation = 0;
            }

            return;
        }

        if (Rank == 5 || Owner.Creature.CombatState is null)
        {
            if (Rank == 5 && _xp >= NeededXp)
            {
                _state = KongQiaoState.TribulationPending;
            }
        }
        else
        {
            CardModel essence = Rank switch
            {
                6 => Owner.Creature.CombatState.CreateCard<QingTiXianYuan>(Owner),
                7 => Owner.Creature.CombatState.CreateCard<HongZaoXianYuan>(Owner),
                8 => Owner.Creature.CombatState.CreateCard<BaiLiXianYuan>(Owner),
                _ => Owner.Creature.CombatState.CreateCard<HuangXingXianYuan>(Owner)
            };

            Flash();
            await CardPileCmd.AddGeneratedCardToCombat(
                essence,
                PileType.Hand,
                Owner,
                CardPilePosition.Bottom);
        }

        if (Rank >= 5
            && _state == KongQiaoState.TribulationPending
            && !IsTribulationDisabled())
        {
            var type = TribulationSystem.GetNextType(Rank, _xp);
            var typeIndex = TribulationSystem.GetTypeIndex(type);
            await PowerCmd.Apply<PlayerTribulationPower>(
                new ThrowingPlayerChoiceContext(),
                Owner.Creature,
                typeIndex + 1,
                Owner.Creature,
                null);
        }
    }

    public override async Task AfterObtained()
    {
        if (Rank >= 6)
        {
            await CreatureCmd.GainMaxHp(Owner.Creature, Rank);
            if (_state == KongQiaoState.XpGathering)
            {
                _state = KongQiaoState.Countdown;
                _battlesToNextTribulation = BattlesPerTribulation;
            }
        }
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
        if (_state == KongQiaoState.TribulationPending)
        {
            if (Rank < 6)
            {
                await ReplaceWithNextStage(0);
                return;
            }

            _xp++;
            if (NextStage is not null && _xp >= NeededXp)
            {
                await ReplaceWithNextStage(_xp - NeededXp);
                return;
            }

            // In the original progression, the third rank-8 tribulation is
            // prepared immediately after the second one succeeds.
            if (Rank == 8 && _xp == 2)
            {
                _state = KongQiaoState.TribulationPending;
                _battlesToNextTribulation = 0;
            }
            else
            {
                _state = KongQiaoState.Countdown;
                _battlesToNextTribulation = BattlesPerTribulation;
            }
            return;
        }

        if (Rank < 6)
        {
            Xp += GetXpReward(room.RoomType);
            if (Xp >= NeededXp)
            {
                if (Rank < 5)
                {
                    await ReplaceWithNextStage(Xp - NeededXp);
                }
                else
                {
                    _state = KongQiaoState.TribulationPending;
                }
            }
            return;
        }

        if (_state == KongQiaoState.Countdown && !IsTribulationDisabled())
        {
            _battlesToNextTribulation--;
            if (_battlesToNextTribulation <= 0)
            {
                _state = KongQiaoState.TribulationPending;
            }
        }
    }

    private async Task ReplaceWithNextStage(int overflowXp)
    {
        var nextStage = NextStage?.ToMutable();
        if (nextStage is not AbstractKongQiaoRelic nextKongQiao)
        {
            return;
        }

        nextKongQiao.Xp = overflowXp;
        nextKongQiao.TribulationState = nextKongQiao.Rank >= 6
            ? (int)KongQiaoState.Countdown
            : (int)KongQiaoState.XpGathering;
        nextKongQiao.BattlesToNextTribulation = nextKongQiao.Rank >= 6
            ? BattlesPerTribulation
            : 0;
        await RelicCmd.Replace(this, nextKongQiao);
        await UpgradeBenMingGuToRank(nextKongQiao.Rank);
    }

    private bool IsTribulationDisabled() =>
        Owner.GetRelic<ShenBuZhi>() is not null;

    private Task UpgradeBenMingGuToRank(int targetRank)
    {
        foreach (var benMingGu in Owner.Deck.Cards
                     .OfType<AbstractBenMingGuCard>()
                     .ToList())
        {
            while (benMingGu.Rank < targetRank && benMingGu.IsUpgradable)
            {
                CardCmd.Upgrade(benMingGu, default);
            }

            Entry.Logger.Info(
                $"Synchronized BenMingGu '{benMingGu.Id.Entry}' to rank {benMingGu.Rank}.");
        }

        return Task.CompletedTask;
    }

    private bool QualifiesForFreePlay(CardModel card)
    {
        return Rank > 1 &&
               Rank <= 5 &&
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
