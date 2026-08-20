using GuZhenRen.Acts;
using GuZhenRen.Monsters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Encounters;

[RegisterActEncounter(typeof(GuZhenRenFinalAct))]
public sealed class LongGongEncounter : ModEncounterTemplate
{
    public override RoomType RoomType => RoomType.Boss;

    public override string BossNodePath =>
        "res://GuZhenRen/images/map/long_gong_boss";

    public override EncounterAssetProfile AssetProfile => new(
        EncounterScenePath:
            "res://GuZhenRen/scenes/encounters/long_gong_encounter.tscn");

    public override IReadOnlyList<string> Slots =>
        ["qi_qiang", "long_qi", "long_gong"];

    public override IEnumerable<MonsterModel> AllPossibleMonsters =>
    [
        ModelDb.Monster<QiQiang>(),
        ModelDb.Monster<YouLongQiQiang>(),
        ModelDb.Monster<LongQi>(),
        ModelDb.Monster<LongGong>()
    ];

    public override float GetCameraScaling() => 0.9f;

    protected override IReadOnlyList<(MonsterModel, string?)>
        GenerateMonsters() =>
    [
        (ModelDb.Monster<QiQiang>().ToMutable(), "qi_qiang"),
        (ModelDb.Monster<LongGong>().ToMutable(), "long_gong")
    ];
}
