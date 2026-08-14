using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
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
    private int _maxHpBonusApplied;

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

    [SavedProperty]
    public int MaxHpBonusApplied
    {
        get => _maxHpBonusApplied;
        set
        {
            AssertMutable();
            _maxHpBonusApplied = Math.Max(0, value);
        }
    }

    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"res://GuZhenRen/images/relics/{RelicImageName}.png",
        IconOutlinePath: $"res://GuZhenRen/images/relics/outline/{RelicImageName}.png",
        BigIconPath: $"res://GuZhenRen/images/relics/{RelicImageName}.png");

    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            var currentAperture = GetCurrentAperture();
            yield return new HoverTip(
                GetProgressLoc("title"),
                currentAperture.BuildProgressDescription(),
                null);
            yield return new HoverTip(
                currentAperture.GetRankTitle(),
                GetProgressLoc("rank_description"),
                null);
        }
    }

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
            await EnsureMaxHpBonusApplied();
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
            LogProgress("tribulation cleared");
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

        // Shen Bu Zhi suppresses the tribulation effect, but it must not
        // freeze the combat countdown that advances aperture progression.
        if (_state == KongQiaoState.Countdown)
        {
            _battlesToNextTribulation--;
            if (_battlesToNextTribulation <= 0)
            {
                _state = KongQiaoState.TribulationPending;
            }
            LogProgress("countdown advanced");
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
        nextKongQiao.MaxHpBonusApplied = MaxHpBonusApplied > 0
            ? MaxHpBonusApplied
            : Rank >= 6 ? Rank : 0;
        await RelicCmd.Replace(this, nextKongQiao);
        await UpgradeBenMingGuToRank(nextKongQiao.Rank);
        ApertureVoiceSystem.PlayForRank(Owner, nextKongQiao.Rank);
    }

    protected virtual bool IsTribulationDisabled() =>
        Owner.GetRelic<ShenBuZhi>() is not null;

    protected Task UpgradeBenMingGuToRank(int targetRank)
    {
        foreach (var benMingGu in Owner.Deck.Cards
                     .OfType<AbstractBenMingGuCard>()
                     .ToList())
        {
            while (benMingGu.Rank < targetRank && benMingGu.IsUpgradable)
            {
                CardCmd.Upgrade(
                    benMingGu,
                    CardPreviewStyle.HorizontalLayout);
            }

            Entry.Logger.Info(
                $"Synchronized BenMingGu '{benMingGu.Id.Entry}' to rank {benMingGu.Rank}.");
        }

        return Task.CompletedTask;
    }

    protected bool QualifiesForFreePlay(CardModel card)
    {
        return Rank > 1 &&
               Rank <= 5 &&
               card.Owner == Owner &&
               card is GuZhenRenCardTemplate guCard &&
               guCard.Rank >= 1 &&
               guCard.Rank < Rank;
    }

    protected static int GetXpReward(RoomType roomType)
    {
        return roomType switch
        {
            RoomType.Monster => 1,
            RoomType.Elite => 3,
            RoomType.Boss => 5,
            _ => 0
        };
    }

    protected async Task EnsureMaxHpBonusApplied()
    {
        if (Rank < 6)
        {
            return;
        }

        var desiredBonus = Rank;
        if (MaxHpBonusApplied >= desiredBonus)
        {
            return;
        }

        var delta = desiredBonus - MaxHpBonusApplied;
        MaxHpBonusApplied = desiredBonus;
        await CreatureCmd.GainMaxHp(Owner.Creature, delta);
    }

    private string BuildProgressDescription()
    {
        if (this is XianTaiGu)
        {
            if (Rank >= 9)
            {
                return GetProgressText("sovereign_max");
            }

            return GetProgressText(
                "sovereign",
                ("Current", Xp),
                ("Needed", NeededXp));
        }

        if (Rank < 5)
        {
            return GetProgressText(
                "mortal",
                ("Current", Xp),
                ("Needed", NeededXp));
        }

        var tribulation = GetTribulationName(
            TribulationSystem.GetNextType(Rank, Xp));
        var tribulationDisabled = IsMutable && IsTribulationDisabled();

        if (Rank == 5)
        {
            if (_state != KongQiaoState.TribulationPending)
            {
                return GetProgressText(
                    "rank_five",
                    ("Current", Xp),
                    ("Needed", NeededXp),
                    ("Tribulation", tribulation));
            }

            return GetProgressText(
                tribulationDisabled
                    ? "rank_five_pending_disabled"
                    : "rank_five_pending",
                ("Tribulation", tribulation));
        }

        if (Rank >= 10)
        {
            return GetProgressText("terminal_complete");
        }

        return _state switch
        {
            KongQiaoState.TribulationPending => GetProgressText(
                tribulationDisabled
                    ? "immortal_pending_disabled"
                    : "immortal_pending",
                ("Current", Xp),
                ("Needed", NeededXp),
                ("Tribulation", tribulation)),
            KongQiaoState.Countdown => GetProgressText(
                tribulationDisabled
                    ? "immortal_countdown_disabled"
                    : "immortal_countdown",
                ("Current", Xp),
                ("Needed", NeededXp),
                ("Battles", BattlesToNextTribulation),
                ("Tribulation", tribulation)),
            _ => GetProgressText(
                "immortal_preparing",
                ("Current", Xp),
                ("Needed", NeededXp),
                ("Tribulation", tribulation))
        };
    }

    private AbstractKongQiaoRelic GetCurrentAperture()
    {
        if (!IsMutable)
        {
            return this;
        }

        return Owner.GetRelic<AbstractKongQiaoRelic>() ?? this;
    }

    private void LogProgress(string reason) => Entry.Logger.Info(
        $"[Aperture] {reason}: rank={Rank}, state={_state}, " +
        $"tribulations={Xp}/{NeededXp}, battles={BattlesToNextTribulation}.");

    private LocString GetRankTitle() => Rank is >= 1 and <= 9
        ? new LocString(
            "card_keywords",
            $"GU_ZHEN_REN_KEYWORD_PIN_JIE_{Rank}.title")
        : GetProgressLoc("rank_10_title");

    private static string GetTribulationName(TribulationType type) =>
        GetProgressLoc($"tribulation_{type.ToString().ToLowerInvariant()}")
            .GetFormattedText();

    private static LocString GetProgressLoc(string suffix) => new(
        "relics",
        $"GU_ZHEN_REN_RELIC_KONG_QIAO_PROGRESS.{suffix}");

    private static string GetProgressText(
        string suffix,
        params (string Name, object Value)[] variables)
    {
        var locString = GetProgressLoc(suffix);
        foreach (var (name, value) in variables)
        {
            locString.AddObj(name, value);
        }

        return locString.GetFormattedText();
    }

}
