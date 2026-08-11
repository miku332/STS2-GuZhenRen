using GuZhenRen.Systems;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class PlayerTribulationPower : ModPowerTemplate
{
    private TribulationDefinition? _definition;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.None;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/PlayerTribulationPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/PlayerTribulationPower_p.png");

    public void FlashEffect() => Flash();

    public override async Task AfterApplied(
        Creature? applier,
        CardModel? cardSource)
    {
        var typeIndex = Math.Clamp((int)Amount - 1, 0, 5);
        var type = (TribulationType)typeIndex;
        _definition = TribulationSystem.Select(type, Owner.Player!);

        Entry.Logger.Info(
            $"[Tribulation] Started {_definition.Type}: {_definition.Name}.");
        await _definition.OnCombatStart(this);
    }

    public override Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        return player == Owner.Player && _definition is not null
            ? _definition.OnPlayerTurnStart(this)
            : Task.CompletedTask;
    }
}
