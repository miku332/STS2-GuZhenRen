using GuZhenRen.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Scaffolding.Godot;

namespace GuZhenRen.Monsters;

[RegisterMonster]
public sealed class YouLongQiQiang : ModMonsterTemplate
{
    private int _movesPerformed;

    public override int MinInitialHp => 500;

    public override int MaxInitialHp => 500;

    public override MonsterAssetProfile AssetProfile => new(
        VisualsScenePath:
            "res://GuZhenRen/scenes/monsters/you_long_qi_qiang.tscn");

    protected override NCreatureVisuals? TryCreateCreatureVisuals() =>
        RitsuGodotNodeFactories.CreateFromScenePath<NCreatureVisuals>(
            AssetProfile.VisualsScenePath!);

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        var context = new ThrowingPlayerChoiceContext();
        await PowerCmd.Apply<QiHuPower>(
            context, Creature, 1, Creature, null);
        await PowerCmd.Apply<YouLongPower>(
            context, Creature, 3, Creature, null);
        await PowerCmd.Apply<GangQiPower>(
            context, Creature, 100, Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var heal = new MoveState(
            "HEAL",
            PerformHeal,
            new HealIntent());
        var strengthen = new MoveState(
            "STRENGTHEN",
            PerformStrengthen,
            new HealIntent(),
            new BuffIntent());
        var branch = new ConditionalBranchState("NEXT_MOVE");
        branch.AddState(heal, () => _movesPerformed >= 4 || _movesPerformed % 2 == 0);
        branch.AddState(strengthen, () => true);

        heal.FollowUpState = branch;
        strengthen.FollowUpState = branch;
        return new MonsterMoveStateMachine([heal, strengthen, branch], heal);
    }

    private async Task PerformHeal(IReadOnlyList<Creature> targets)
    {
        _movesPerformed++;
        await CreatureCmd.Heal(Creature, 40);
    }

    private async Task PerformStrengthen(IReadOnlyList<Creature> targets)
    {
        _movesPerformed++;
        await CreatureCmd.Heal(Creature, 20);
        await PowerCmd.Apply<YouLongPower>(
            new ThrowingPlayerChoiceContext(),
            Creature,
            1,
            Creature,
            null);
    }
}
