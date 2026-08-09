using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class XianGuCanHai : ModRelicTemplate
{
    private const string ExtraSmithKey = "ExtraSmith";

    private int _counter = 1;

    public override RelicRarity Rarity => RelicRarity.Event;

    public override bool ShowCounter => true;

    public override int DisplayAmount => Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(ExtraSmithKey, 1m)
    ];

    [SavedProperty]
    public int Counter
    {
        get => _counter;
        set
        {
            AssertMutable();
            _counter = Math.Max(0, value);
            DynamicVars[ExtraSmithKey].BaseValue = _counter;
            Status = _counter > 0 ? RelicStatus.Normal : RelicStatus.Disabled;
            InvokeDisplayAmountChanged();
        }
    }

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/relics/XianGuCanHai.png",
        IconOutlinePath: "res://GuZhenRen/images/relics/outline/XianGuCanHai.png",
        BigIconPath: "res://GuZhenRen/images/relics/XianGuCanHai.png");

    public override Task AfterObtained()
    {
        if (!IsMutable)
        {
            return Task.CompletedTask;
        }

        if (Counter <= 0)
        {
            Counter = 1;
        }
        else
        {
            DynamicVars[ExtraSmithKey].BaseValue = Counter;
            Status = RelicStatus.Normal;
        }

        return Task.CompletedTask;
    }
}
