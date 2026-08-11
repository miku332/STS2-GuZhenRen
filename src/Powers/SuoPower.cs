using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class SuoPower : ModPowerTemplate
{
    private const int Threshold = 7;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/SuoPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/SuoPower_p.png");

    public override Task AfterApplied(
        Creature? applier,
        CardModel? cardSource)
    {
        SetAmount(0);
        return Task.CompletedTask;
    }

    public override Task AfterCardPlayed(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay) => CountCard<DexterityPower>(choiceContext, cardPlay);

    private async Task CountCard<TPower>(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
        where TPower : PowerModel
    {
        if (!Owner.IsAlive || cardPlay.Card.Owner.Creature != GetAffectedPlayer())
        {
            return;
        }

        var next = Amount + 1;
        if (next < Threshold)
        {
            SetAmount(next);
            return;
        }

        SetAmount(0);
        Flash();
        await PowerCmd.Apply<TPower>(
            choiceContext,
            cardPlay.Card.Owner.Creature,
            -1,
            Owner,
            cardPlay.Card);
    }

    private Creature? GetAffectedPlayer() =>
        Applier?.Player is not null
            ? Applier
            : Owner.CombatState?.Players.FirstOrDefault()?.Creature;
}
