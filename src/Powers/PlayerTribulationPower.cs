using GuZhenRen.Systems;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.Relics;

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
        _definition = ResolveDefinition();

        Entry.Logger.Info(
            $"[Tribulation] Started {_definition.Type}: {_definition.Name}.");
        await _definition.OnCombatStart(this);
    }

    public override Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        return player == Owner.Player
            ? ResolveDefinition().OnPlayerTurnStart(this)
            : Task.CompletedTask;
    }

    private TribulationDefinition ResolveDefinition()
    {
        if (_definition is not null)
        {
            return _definition;
        }

        if (TribulationSystem.TryDecodeDefinition(Amount, out var definition))
        {
            return _definition = definition;
        }

        TribulationType type;
        if (Amount is >= 1 and <= 6)
        {
            // Compatibility for saves that only stored the tribulation type.
            type = (TribulationType)((int)Amount - 1);
        }
        else if (Owner.Player?.GetRelic<AbstractKongQiaoRelic>() is { } aperture)
        {
            type = TribulationSystem.GetNextType(aperture.Rank, aperture.Xp);
            Entry.Logger.Warn(
                $"[Tribulation] Invalid encoded amount {Amount}; " +
                $"falling back to {type} for rank {aperture.Rank}, progress {aperture.Xp}.");
        }
        else
        {
            type = TribulationType.Earthly;
            Entry.Logger.Warn(
                $"[Tribulation] Invalid encoded amount {Amount} without an aperture; " +
                "falling back to Earthly.");
        }

        return _definition = TribulationSystem.Select(type, Owner.Player!);
    }
}
