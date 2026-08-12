using System.Collections.Generic;
using GuZhenRen.CardPools;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class XianTaiGu : AbstractKongQiaoRelic
{
    private int _currentRank = 1;

    [SavedProperty]
    public int CurrentRank
    {
        get => _currentRank;
        set
        {
            AssertMutable();
            _currentRank = Math.Clamp(value, 1, 9);
            RefreshDynamicVars();
        }
    }

    public override int Rank => CurrentRank;

    protected override int NeededXp => CurrentRank < 9 ? CurrentRank : int.MaxValue;

    protected override string RelicImageName => "XianTaiGu";

    protected override RelicModel? NextStage => null;

    public override RelicRarity Rarity => RelicRarity.Starter;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("RemainingXp", 0),
        new StringVar(
            "RankStatus",
            GetLocalizedText("preview_status")),
        new StringVar(
            "RankEffectDescription",
            GetLocalizedText("preview_effect"))
    ];

    public override async Task AfterObtained()
    {
        RefreshDynamicVars();
        await EnsureMaxHpBonusApplied();
    }

    public override async Task AfterCombatVictory(CombatRoom room)
    {
        if (CurrentRank >= 9)
        {
            return;
        }

        Xp += GetXpReward(room.RoomType);
        if (Xp < NeededXp)
        {
            RefreshDynamicVars();
            return;
        }

        Xp -= NeededXp;
        CurrentRank++;
        await EnsureMaxHpBonusApplied();
        await UpgradeBenMingGuToRank(CurrentRank);
        Flash();
        Entry.Logger.Info($"XianTaiGu advanced to rank {CurrentRank} without tribulation.");
    }

    protected override void AfterCloned()
    {
        base.AfterCloned();
        RefreshDynamicVars();
    }

    protected override bool IsTribulationDisabled() => true;

    private static string GetLocalizedText(string suffix) =>
        new LocString(
            "relics",
            $"GU_ZHEN_REN_RELIC_XIAN_TAI_GU.{suffix}")
        .GetFormattedText();

    private void RefreshDynamicVars()
    {
        if (!IsMutable)
        {
            return;
        }

        var rankStatus = new LocString(
            "relics",
            $"GU_ZHEN_REN_RELIC_XIAN_TAI_GU.rank_{CurrentRank}_status")
            .GetFormattedText();
        var effect = new LocString(
            "relics",
            $"GU_ZHEN_REN_RELIC_XIAN_TAI_GU.rank_{CurrentRank}_effect")
            .GetFormattedText();
        var noTribulation = new LocString(
            "relics",
            "GU_ZHEN_REN_RELIC_XIAN_TAI_GU.no_tribulation")
            .GetFormattedText();

        DynamicVars["RemainingXp"].BaseValue = Math.Max(0, NeededXp - Xp);
        ((StringVar)DynamicVars["RankStatus"]).StringValue = rankStatus;
        ((StringVar)DynamicVars["RankEffectDescription"]).StringValue =
            string.IsNullOrEmpty(effect)
                ? noTribulation
                : $"{effect}\n{noTribulation}";
    }
}
