using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class ZhiDaoDaoHenPower : AbstractDaoHenPower
{
    [SavedProperty]
    public int StoredEnergy { get; set; }

    public override async Task BeforeFlushLate(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (player != Owner.Player || Amount <= 0)
        {
            return;
        }

        if (player.GetRelic<IceCream>() is null)
        {
            StoredEnergy = Math.Min(Amount, (int)player.GetEnergy());
        }

        if (player.Creature.CombatState is not { } combatState
            || !Hook.ShouldFlush(combatState, player))
        {
            return;
        }

        var selectedCards = await CardSelectCmd.FromHand(
            choiceContext,
            player,
            new CardSelectorPrefs(SelectionScreenPrompt, 0, Amount),
            static card => !card.ShouldRetainThisTurn,
            this);

        foreach (var card in selectedCards)
        {
            card.GiveSingleTurnRetain();
        }

        if (selectedCards.Any() || StoredEnergy > 0)
        {
            Flash();
        }
    }

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        Player player)
    {
        if (player.Creature != Owner)
        {
            return;
        }

        var energy = StoredEnergy;
        StoredEnergy = 0;
        await ResetToBianHua(choiceContext, player);

        if (energy > 0)
        {
            Flash();
            await PlayerCmd.GainEnergy(energy, player);
        }
    }
}
