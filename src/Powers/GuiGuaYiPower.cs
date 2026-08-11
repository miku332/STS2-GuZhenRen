using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class GuiGuaYiPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.None;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/GuiGuaYiPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/GuiGuaYiPower_p.png");

    public override Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        return player.Creature == GetAffectedPlayer()
            ? SyncToCurrentIntent(choiceContext)
            : Task.CompletedTask;
    }

    public async Task SyncToCurrentIntent(PlayerChoiceContext choiceContext)
    {
        if (!Owner.IsAlive || Owner.Monster is null)
        {
            return;
        }

        var intangible = Owner.GetPower<IntangiblePower>();
        if (Owner.Monster.IntendsToAttack)
        {
            if (intangible is not null)
            {
                await PowerCmd.Remove(intangible);
            }

            return;
        }

        if (intangible is null)
        {
            Flash();
            await PowerCmd.Apply<IntangiblePower>(
                choiceContext,
                Owner,
                1,
                Owner,
                null);
        }
    }

    private Creature? GetAffectedPlayer() =>
        Applier?.Player is not null
            ? Applier
            : Owner.CombatState?.Players.FirstOrDefault()?.Creature;
}
