using Godot;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Unlocks;
using GuZhenRen.Encounters;

namespace GuZhenRen.Acts;

public sealed class GuZhenRenFinalAct : ActModel
{
    protected override int BaseNumberOfRooms => 1;

    public override int Index => 3;

    public override bool IsDefault => false;

    public new LocString Title =>
        new("acts", "GU_ZHEN_REN_FINAL_ACT.title");

    public override IEnumerable<EncounterModel> GenerateAllEncounters() =>
    [
        ModelDb.Encounter<LongGongEncounter>(),
        ModelDb.Encounter<ByrdonisElite>()
    ];

    public override IEnumerable<AncientEventModel> AllAncients =>
    [
        ModelDb.AncientEvent<Neow>()
    ];

    public override IEnumerable<AncientEventModel> GetUnlockedAncients(
        UnlockState state) => AllAncients;

    public override string[] BgMusicOptions =>
    [
        "event:/music/act3_boss_queen",
        "event:/music/act3_boss_queen"
    ];

    public override string[] MusicBankPaths =>
    [
        "res://banks/desktop/act3_a1.bank",
        "res://banks/desktop/act3_a2.bank"
    ];

    public override string AmbientSfx => "event:/sfx/ambience/act3_ambience";

    public override Color MapBgColor => new(0f, 0f, 0f, 0f);

    public override Color MapTraveledColor => new("ffffff");

    public override Color MapUntraveledColor => new("b0b0b0");

    public override string ChestSpineSkinNameNormal => "act3";

    public override string ChestSpineSkinNameStroke => "act3_stroke";

    public override string ChestOpenSfx => "event:/sfx/ui/treasure/treasure_act3";

    public override IEnumerable<EncounterModel> BossDiscoveryOrder =>
    [
        ModelDb.Encounter<LongGongEncounter>()
    ];

    public override IEnumerable<EventModel> AllEvents => [];

    public override bool IsUnlocked(UnlockState unlockState) => true;

    protected override void ApplyActDiscoveryOrderModifications(
        UnlockState unlockState)
    {
    }

    public override MapPointTypeCounts GetMapPointTypes(Rng mapRng) =>
        new(0, 0);
}
