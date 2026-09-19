using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using GuZhenRen.Powers;
using GuZhenRen.Patches;
using GuZhenRen.Systems;
using GuZhenRen.Multiplayer;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interactions.RightClick;
using STS2RitsuLib.Networking.ManagedActions;
using GuZhenRen.Cards;

namespace GuZhenRen.Relics;

public abstract class AbstractKongQiaoRelic : ModRelicTemplate, IModRightClickableRelic
{
    private enum KongQiaoState
    {
        XpGathering,
        TribulationPending,
        Countdown,
        ReadyToTribulate
    }

    private const int BattlesPerTribulation = 2;
    private bool _effectUsedThisCombat;
    private int _xp;
    private KongQiaoState _state;
    private int _battlesToNextTribulation;
    private int _maxHpBonusApplied;

    public abstract int Rank { get; }

    public override bool ShowCounter => true;

    public override int DisplayAmount => Rank;

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

            if (currentAperture.ShouldShowTribulationHoverTip)
            {
                var tribulationType = TribulationSystem.GetNextType(
                    currentAperture.Rank,
                    currentAperture.Xp);
                var keywordStem = GetTribulationKeywordStem(tribulationType);
                yield return new HoverTip(
                    new LocString(
                        "card_keywords",
                        $"GU_ZHEN_REN_KEYWORD_{keywordStem}.title"),
                    new LocString(
                        "card_keywords",
                        $"GU_ZHEN_REN_KEYWORD_{keywordStem}.description"));
            }

            yield return new HoverTip(
                currentAperture.GetRankTitle(),
                currentAperture.GetRankDescription(),
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
            if (Rank == 5
                && _xp >= NeededXp
                && _state != KongQiaoState.ReadyToTribulate)
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

        if (Rank >= 10)
        {
            ResetTerminalTribulationState();
            return;
        }

        if (Rank >= 5
            && _state == KongQiaoState.TribulationPending
            && !IsTribulationDisabled())
        {
            var type = TribulationSystem.GetNextType(Rank, _xp);
            var definition = TribulationSystem.Select(type, Owner);
            await PowerCmd.Apply<PlayerTribulationPower>(
                new ThrowingPlayerChoiceContext(),
                Owner.Creature,
                TribulationSystem.EncodeDefinition(definition),
                Owner.Creature,
                null);
        }
    }

    public override async Task AfterObtained()
    {
        if (Rank < 6)
        {
            return;
        }

        await EnsureMaxHpBonusApplied();
        if (Rank >= 10)
        {
            ResetTerminalTribulationState();
            return;
        }

        if (_state == KongQiaoState.XpGathering)
        {
            _state = KongQiaoState.Countdown;
            _battlesToNextTribulation = BattlesPerTribulation;
        }
    }

    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        BenMingGuRankProtection.EnsureMinimumRank(Owner);
        await BenMingGuUniquenessPatch.EnforceDeckUniqueness(Owner);
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
        if (Rank >= 10)
        {
            ResetTerminalTribulationState();
            return;
        }

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

        if (_state == KongQiaoState.ReadyToTribulate)
        {
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
        nextKongQiao.TribulationState = nextKongQiao.Rank is >= 6 and < 10
            ? (int)KongQiaoState.Countdown
            : (int)KongQiaoState.XpGathering;
        nextKongQiao.BattlesToNextTribulation = nextKongQiao.Rank is >= 6 and < 10
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

    public bool CanHandleRightClickLocal(ModRightClickContext context) =>
        context.Player == Owner
        && Owner.Creature.IsAlive
        && IsMutable
        && !CombatManager.Instance.IsInProgress
        && !IsTribulationDisabled()
        && CanToggleTribulationState();

    public async Task OnRightClick(ModRightClickExecutionContext context)
    {
        if (!CanHandleRightClickLocal(new ModRightClickContext(
                context.Player,
                context.Model,
                context.Trigger)))
        {
            return;
        }

        var payload = new KongQiaoTribulationTogglePayload(context.Player.NetId);
        if (RunManager.Instance.IsSingleplayerOrFakeMultiplayer)
        {
            ToggleTribulationState();
            await Task.CompletedTask;
            return;
        }

        NetGuZhenRenActions.RequestWithRetry(
            () => NetGuZhenRenActions.RequestKongQiaoTribulationToggle(payload),
            () => context.Player.GetRelic<AbstractKongQiaoRelic>() is not null,
            () => Entry.Logger.Warn("Kong Qiao tribulation toggle action was not queued."),
            "Kong Qiao tribulation toggle",
            requiresCombat: false);
        await Task.CompletedTask;
    }

    internal static Task ExecuteManagedTribulationToggleAsync(
        RitsuLibManagedNetActionContext<KongQiaoTribulationTogglePayload> context)
    {
        if (context.Message.OwnerNetId != context.Player.NetId
            || context.Player.GetRelic<AbstractKongQiaoRelic>() is not { } relic
            || !relic.CanToggleTribulationState())
        {
            return Task.CompletedTask;
        }

        relic.ToggleTribulationState();
        return Task.CompletedTask;
    }

    private bool CanToggleTribulationState() =>
        (Rank == 5 || (Rank == 8 && Xp >= 2))
        && _state is KongQiaoState.TribulationPending or KongQiaoState.ReadyToTribulate;

    private void ToggleTribulationState()
    {
        _state = _state == KongQiaoState.TribulationPending
            ? KongQiaoState.ReadyToTribulate
            : KongQiaoState.TribulationPending;
        Flash();
        InvokeDisplayAmountChanged();
    }

    private void ResetTerminalTribulationState()
    {
        _state = KongQiaoState.XpGathering;
        _battlesToNextTribulation = 0;
    }

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
                    LocalContext.IsMe(Owner)
                        ? CardPreviewStyle.HorizontalLayout
                        : CardPreviewStyle.None);
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
                ("Remaining", Math.Max(0, NeededXp - Xp)));
        }

        var tribulation = GetTribulationName(
            TribulationSystem.GetNextType(Rank, Xp));
        var tribulationDisabled = IsMutable && IsTribulationDisabled();

        if (Rank == 5)
        {
            if (_state == KongQiaoState.ReadyToTribulate)
            {
                return GetProgressText("ready_to_tribulate");
            }

            if (_state != KongQiaoState.TribulationPending)
            {
                return GetProgressText(
                    "rank_five",
                    ("Remaining", Math.Max(0, NeededXp - Xp)),
                    ("Tribulation", tribulation));
            }

            var rankFiveDescription = GetProgressText(
                tribulationDisabled
                    ? "rank_five_pending_disabled"
                    : "rank_five_pending",
                ("Tribulation", tribulation));

            if (!tribulationDisabled && !IsCombatActive())
            {
                rankFiveDescription += GetProgressText("cancel_immortal");
            }

            return rankFiveDescription;
        }

        if (Rank >= 10)
        {
            return GetProgressText("terminal_complete");
        }

        if (_state == KongQiaoState.TribulationPending)
        {
            if (tribulationDisabled)
            {
                return GetProgressText(
                    "immortal_pending_disabled",
                    ("Tribulation", tribulation));
            }

            var description = IsCombatActive()
                ? GetProgressText("tribulation_active", ("Tribulation", tribulation))
                : GetProgressText("immortal_pending", ("Tribulation", tribulation));

            if (!IsCombatActive())
            {
                description += BuildBreakthroughHint(tribulation);
                if (Rank == 8 && Xp == 2)
                {
                    description += GetProgressText("cancel_venerable");
                }
            }

            return description;
        }

        if (_state == KongQiaoState.Countdown)
        {
            return GetProgressText(
                tribulationDisabled
                    ? "immortal_countdown_disabled"
                    : "immortal_countdown",
                ("Battles", BattlesToNextTribulation),
                ("Tribulation", tribulation));
        }

        if (_state == KongQiaoState.ReadyToTribulate)
        {
            return GetProgressText("ready_to_tribulate");
        }

        return GetProgressText("immortal_preparing", ("Tribulation", tribulation));
    }

    private string BuildBreakthroughHint(string tribulation)
    {
        if (Rank == 6 && Xp == 1)
        {
            return GetProgressText(
                "breakthrough_to_rank",
                ("Tribulation", tribulation),
                ("Rank", GetRankName(7)));
        }

        if (Rank == 7 && Xp == 1)
        {
            return GetProgressText(
                "breakthrough_to_rank",
                ("Tribulation", tribulation),
                ("Rank", GetRankName(8)));
        }

        if (Rank == 8 && Xp == 1)
        {
            return GetProgressText("breakthrough_to_venerable", ("Tribulation", tribulation));
        }

        if (Rank == 8 && Xp == 2)
        {
            return GetProgressText(
                "breakthrough_to_rank",
                ("Tribulation", tribulation),
                ("Rank", GetRankName(9)));
        }

        if (Rank == 9 && Xp == 17)
        {
            return GetProgressText(
                "breakthrough_to_rank",
                ("Tribulation", tribulation),
                ("Rank", GetRankName(10)));
        }

        return string.Empty;
    }

    private string GetRankName(int rank) => rank == 10
        ? GetProgressLoc("rank_10_title").GetFormattedText()
        : new LocString(
            "card_keywords",
            $"GU_ZHEN_REN_KEYWORD_PIN_JIE_{rank}.title").GetFormattedText();

    private bool IsCombatActive() => CombatManager.Instance.IsInProgress;

    private bool ShouldShowTribulationHoverTip =>
        _state is KongQiaoState.Countdown or KongQiaoState.TribulationPending;

    private static string GetTribulationKeywordStem(TribulationType type) => type switch
    {
        TribulationType.Earthly => "TRIBULATION_EARTHLY",
        TribulationType.Heavenly => "TRIBULATION_HEAVENLY",
        TribulationType.Grand => "TRIBULATION_GRAND",
        TribulationType.Myriad => "TRIBULATION_MYRIAD",
        TribulationType.MinorChaos => "TRIBULATION_MINOR_CHAOS",
        TribulationType.MajorChaos => "TRIBULATION_MAJOR_CHAOS",
        _ => "TRIBULATION_EARTHLY"
    };

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

    private LocString GetRankDescription() => Rank is >= 1 and <= 9
        ? new LocString(
            "card_keywords",
            "GU_ZHEN_REN_KEYWORD_PIN_JIE.description")
        : GetProgressLoc("rank_10_description");

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
