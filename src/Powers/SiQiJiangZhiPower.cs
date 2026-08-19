using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class SiQiJiangZhiPower : ModPowerTemplate
{
    private const int VantomStrength = 10;

    private bool _isVantom;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType =>
        _isVantom ? PowerStackType.Single : PowerStackType.Counter;

    public override bool AllowNegative => true;

    public override LocString Description
    {
        get
        {
            var description = new LocString(
                "powers",
                _isVantom
                    ? "GU_ZHEN_REN_POWER_SI_QI_JIANG_ZHI_POWER.vantom_description"
                    : "GU_ZHEN_REN_POWER_SI_QI_JIANG_ZHI_POWER.description");
            if (!_isVantom)
            {
                description.Add("Amount", Amount);
            }

            return description;
        }
    }

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/SiQiJiangZhiPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/SiQiJiangZhiPower_p.png");

    public override Task AfterApplied(
        Creature? applier,
        CardModel? cardSource)
    {
        _isVantom = Owner.Monster?.GetType().Name == "Vantom";
        if (_isVantom)
        {
            SetAmount(-1);
        }

        return Task.CompletedTask;
    }

    public override async Task AfterSideTurnStart(
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (_isVantom)
        {
            return;
        }

        if (side != CombatSide.Enemy
            || !Owner.IsAlive
            || !participants.Contains(Owner))
        {
            return;
        }

        if (Amount > 1)
        {
            Flash();
            await PowerCmd.Decrement(this);
            return;
        }

        var target = Applier?.Player is not null && Applier.IsAlive
            ? Applier
            : combatState.Players.FirstOrDefault(player =>
                player.Creature.IsAlive)?.Creature;
        if (target is null)
        {
            return;
        }

        Flash();
        await CreatureCmd.Kill(target);
        if (Owner.IsAlive)
        {
            await PowerCmd.Remove(this);
        }
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (!_isVantom
            || side != CombatSide.Enemy
            || !Owner.IsAlive
            || !participants.Contains(Owner))
        {
            return;
        }

        Flash();
        await PowerCmd.Apply<StrengthPower>(
            choiceContext,
            Owner,
            VantomStrength,
            Owner,
            null);
    }
}
