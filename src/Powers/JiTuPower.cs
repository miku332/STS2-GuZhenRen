using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class JiTuPower : ModPowerTemplate
{
    private const int SturdyClampRetainedBlock = 10;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/JiTuPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/JiTuPower_p.png");

    public override bool ShouldClearBlock(Creature creature)
    {
        if (creature == Owner && Amount > 0)
        {
            return false;
        }

        return true;
    }

    public override async Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IReadOnlyList<Creature> participants,
        ICombatState combatState)
    {
        if (Amount <= 0
            || !participants.Contains(Owner)
            || Owner.GetPower<BarricadePower>() is not null
            || Owner.GetPower<BlurPower>() is not null)
        {
            return;
        }

        var sturdyClampRetain = HasSturdyClamp ? SturdyClampRetainedBlock : 0;
        var retainAmount = Math.Max(Amount, sturdyClampRetain);

        if (Owner.Block <= retainAmount)
        {
            return;
        }

        Flash();
        await CreatureCmd.LoseBlock(
            choiceContext,
            Owner,
            Owner.Block - retainAmount,
            Owner);
    }

    private bool HasSturdyClamp =>
        Owner.Player?.Relics.Any(relic => relic.Id.Entry == "STURDY_CLAMP") == true;
}
