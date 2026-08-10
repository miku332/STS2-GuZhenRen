using GuZhenRen.CardPools;
using GuZhenRen.Powers;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class JianChiGu : ModRelicTemplate
{
    private readonly HashSet<PowerModel> _perseveredPowers = [];

    public override RelicRarity Rarity => RelicRarity.Event;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/relics/JianChiGu.png",
        IconOutlinePath: "res://GuZhenRen/images/relics/outline/JianChiGu.png",
        BigIconPath: "res://GuZhenRen/images/relics/JianChiGu.png");

    public override Task BeforeCombatStart()
    {
        _perseveredPowers.Clear();
        return Task.CompletedTask;
    }

    internal bool TryPreserve(PowerModel power)
    {
        if (power.Owner != Owner.Creature
            || power.Type != PowerType.Buff
            || power is AbstractDaoHenPower
            || power.Amount != 1
            || power.SkipNextDurationTick
            || !_perseveredPowers.Add(power))
        {
            return false;
        }

        Flash();
        return true;
    }
}
