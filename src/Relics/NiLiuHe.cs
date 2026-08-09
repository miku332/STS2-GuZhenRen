using GuZhenRen.CardPools;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class NiLiuHe : ModRelicTemplate
{
    public const int MaxWater = 9;

    private int _counter = 3;

    public override RelicRarity Rarity => RelicRarity.Event;

    public override bool ShowCounter => true;

    public override int DisplayAmount => Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Water", 3),
        new DynamicVar("MaxWater", MaxWater)
    ];

    [SavedProperty]
    public int Counter
    {
        get => _counter;
        set
        {
            AssertMutable();
            _counter = Math.Clamp(value, 0, MaxWater);
            DynamicVars["Water"].BaseValue = _counter;
            Status = _counter > 0 ? RelicStatus.Normal : RelicStatus.Disabled;
            InvokeDisplayAmountChanged();
        }
    }

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/relics/NiLiuHe.png",
        IconOutlinePath: "res://GuZhenRen/images/relics/outline/NiLiuHe.png",
        BigIconPath: "res://GuZhenRen/images/relics/NiLiuHe.png");

    public override Task AfterObtained()
    {
        Counter = Counter;
        return Task.CompletedTask;
    }

    public override Task AfterRoomEntered(AbstractRoom room)
    {
        AddWater(1);
        return Task.CompletedTask;
    }

    public bool AddWater(int amount)
    {
        if (amount <= 0 || Counter >= MaxWater)
        {
            return false;
        }

        Counter += amount;
        Flash();
        return true;
    }

    public bool TryConsumeWater()
    {
        if (Counter <= 0)
        {
            return false;
        }

        Counter--;
        Flash();
        return true;
    }
}
