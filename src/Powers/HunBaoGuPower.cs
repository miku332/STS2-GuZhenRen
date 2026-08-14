using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class HunBaoGuPower : ModPowerTemplate
{
    private bool _exploded;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/HunBaoGuPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/HunBaoGuPower_p.png");

    public override Task BeforeDeath(Creature creature)
    {
        return creature == Owner ? Explode() : Task.CompletedTask;
    }

    private async Task Explode()
    {
        if (_exploded || Amount <= 0 || Owner.CombatState is not { } combatState)
        {
            return;
        }

        _exploded = true;
        Flash();
        var targets = combatState.Players
            .Select(player => player.Creature)
            .Concat(combatState.Enemies)
            .Where(creature => creature != Owner && creature.IsAlive)
            .ToList();
        if (targets.Count == 0)
        {
            return;
        }

        await CreatureCmd.Damage(
            new ThrowingPlayerChoiceContext(),
            targets,
            Amount,
            ValueProp.Unpowered,
            null,
            null,
            null);
    }
}
