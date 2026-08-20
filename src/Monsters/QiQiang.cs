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
public sealed class QiQiang : ModMonsterTemplate
{
    private const int HealAmount = 30;

    public override int MinInitialHp => 250;

    public override int MaxInitialHp => 250;

    public override MonsterAssetProfile AssetProfile => new(
        VisualsScenePath: "res://GuZhenRen/scenes/monsters/qi_qiang.tscn");

    protected override NCreatureVisuals? TryCreateCreatureVisuals() =>
        RitsuGodotNodeFactories.CreateFromScenePath<NCreatureVisuals>(
            AssetProfile.VisualsScenePath!);

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<QiHuPower>(
            new ThrowingPlayerChoiceContext(),
            Creature,
            1,
            Creature,
            null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var heal = new MoveState(
            "HEAL",
            PerformHeal,
            new HealIntent());
        heal.FollowUpState = heal;
        return new MonsterMoveStateMachine([heal], heal);
    }

    private Task PerformHeal(IReadOnlyList<Creature> targets) =>
        CreatureCmd.Heal(Creature, HealAmount);
}
