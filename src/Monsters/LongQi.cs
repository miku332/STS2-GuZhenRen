using GuZhenRen.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Scaffolding.Godot;

namespace GuZhenRen.Monsters;

[RegisterMonster]
public sealed class LongQi : ModMonsterTemplate
{
    public override int MinInitialHp => 125;

    public override int MaxInitialHp => 125;

    public override MonsterAssetProfile AssetProfile => new(
        VisualsScenePath: "res://GuZhenRen/scenes/monsters/long_qi.tscn");

    protected override NCreatureVisuals? TryCreateCreatureVisuals() =>
        RitsuGodotNodeFactories.CreateFromScenePath<NCreatureVisuals>(
            AssetProfile.VisualsScenePath!);

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var qiBao = new MoveState(
            "QIAN_LONG_QI_BAO",
            PerformQiBao,
            new MultiAttackIntent(6, 2));
        var longQi = new MoveState(
            "LONG_QI",
            PerformLongQi,
            new BuffIntent());
        qiBao.FollowUpState = longQi;
        longQi.FollowUpState = qiBao;
        return new MonsterMoveStateMachine([qiBao, longQi], qiBao);
    }

    private Task PerformQiBao(IReadOnlyList<Creature> targets) =>
        DamageCmd.Attack(6)
            .WithHitCount(2)
            .FromMonster(this)
            .WithNoAttackerAnim()
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(null);

    private async Task PerformLongQi(IReadOnlyList<Creature> targets)
    {
        var context = new ThrowingPlayerChoiceContext();
        await PowerCmd.Apply<StrengthPower>(
            context, Creature, 2, Creature, null);

        var longGong = CombatState.Enemies.FirstOrDefault(enemy =>
            enemy.IsAlive && enemy.Monster is LongGong);
        await PowerCmd.Apply<JiuLongWenHuShenPower>(
            context,
            longGong ?? Creature,
            4,
            Creature,
            null);
    }
}
