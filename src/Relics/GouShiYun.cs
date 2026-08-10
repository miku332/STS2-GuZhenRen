using System.Collections.Generic;
using System.Threading.Tasks;
using GuZhenRen.CardPools;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class GouShiYun : ModRelicTemplate
{
    public const int BaseChance = 25;
    public const int HongYunBonus = 15;

    private const string ChanceKey = "Chance";

    public override RelicRarity Rarity => RelicRarity.Uncommon;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(ChanceKey, BaseChance)
    ];

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/relics/GouShiYun.png",
        IconOutlinePath: "res://GuZhenRen/images/relics/outline/GouShiYun.png",
        BigIconPath: "res://GuZhenRen/images/relics/GouShiYun.png");

    public override Task AfterObtained()
    {
        RefreshChance();
        return Task.CompletedTask;
    }

    public override Task AfterRoomEntered(AbstractRoom room)
    {
        RefreshChance();
        return Task.CompletedTask;
    }

    public override Task AfterCombatVictory(CombatRoom room)
    {
        if (!room.Encounter.ShouldGiveRewards || !RollExtraRelic())
        {
            return Task.CompletedTask;
        }

        Owner.PopulateRelicGrabBagIfNecessary(Owner.RunState.Rng.UpFront);
        room.AddExtraReward(Owner, new RelicReward(RelicRarity.None, Owner));
        Flash();
        return Task.CompletedTask;
    }

    internal void RefreshChance()
    {
        DynamicVars[ChanceKey].BaseValue = GetChance();
    }

    private int GetChance() =>
        Owner.GetRelic<HongYunQiTianGu>() is null
            ? BaseChance
            : BaseChance + HongYunBonus;

    private bool RollExtraRelic() =>
        Owner.PlayerRng.Rewards.NextFloat(100f) < GetChance();
}
