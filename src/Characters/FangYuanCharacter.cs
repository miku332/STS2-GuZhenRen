using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Godot;
using GuZhenRen.CardPools;
using GuZhenRen.Cards;
using GuZhenRen.Relics;

namespace GuZhenRen.Characters;

[RegisterCharacter]
public sealed class FangYuanCharacter :
    ModCharacterTemplate<GuZhenRenCardPool, GuZhenRenRelicPool, GuZhenRenPotionPool>
{
    public override Color NameColor => new(0.72f, 0.72f, 0.76f);

    public override Color EnergyLabelOutlineColor => new(0.15f, 0.15f, 0.15f);

    public override Color MapDrawingColor => new(0.72f, 0.72f, 0.76f);

    public override CharacterGender Gender => CharacterGender.Masculine;

    public override int StartingHp => 80;

    public override int StartingGold => 99;

    public override CharacterAssetProfile AssetProfile => CharacterAssetProfiles.Merge(
        CharacterAssetProfiles.Ironclad(),
        new(
            Scenes: new(
                VisualsPath: "res://GuZhenRen/scenes/fang_yuan_character.tscn",
                EnergyCounterPath: "res://GuZhenRen/scenes/fang_yuan_energy_counter.tscn",
                MerchantAnimPath: "res://GuZhenRen/images/characters/FangYuan/Idle.png",
                RestSiteAnimPath: "res://GuZhenRen/images/characters/FangYuan/Idle.png"),
            Ui: new(
                IconTexturePath: "res://GuZhenRen/images/characters/FangYuan/Button.png",
                IconPath: "res://GuZhenRen/scenes/fang_yuan_icon.tscn",
                CharacterSelectBgPath: "res://GuZhenRen/scenes/fang_yuan_bg.tscn",
                CharacterSelectIconPath: "res://GuZhenRen/images/character_select/char_select_fang_yuan.png",
                CharacterSelectLockedIconPath: "res://GuZhenRen/images/character_select/char_select_fang_yuan_locked.png",
                MapMarkerPath: "res://GuZhenRen/images/ui/map_marker_fang_yuan.png")));

    public override float AttackAnimDelay => 0.15f;

    public override float CastAnimDelay => 0.2f;

    protected override NCreatureVisuals? TryCreateCreatureVisuals() =>
        RitsuGodotNodeFactories.CreateFromScenePath<NCreatureVisuals>(
            AssetProfile.Scenes!.VisualsPath!);

    public override List<string> GetArchitectAttackVfx() =>
    [
        "vfx/vfx_attack_blunt",
        "vfx/vfx_heavy_blunt",
        "vfx/vfx_attack_slash"
    ];
}
