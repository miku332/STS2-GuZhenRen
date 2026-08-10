using System.Collections.Generic;
using System.Threading.Tasks;
using GuZhenRen.CardPools;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class FeiGuDunGu : ModRelicTemplate
{
    private const string UsesKey = "Uses";
    private const int MaxUses = 3;
    private int _counter = MaxUses;

    public override RelicRarity Rarity => RelicRarity.Event;

    public override bool ShowCounter => true;

    public override int DisplayAmount => Counter;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar(UsesKey, MaxUses)
    ];

    [SavedProperty]
    public int Counter
    {
        get => _counter;
        set
        {
            AssertMutable();
            _counter = Math.Clamp(value, 0, MaxUses);
            DynamicVars[UsesKey].BaseValue = _counter;
            Status = _counter > 0 ? RelicStatus.Normal : RelicStatus.Disabled;
            InvokeDisplayAmountChanged();
        }
    }

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/relics/FeiGuDunGu.png",
        IconOutlinePath: "res://GuZhenRen/images/relics/outline/FeiGuDunGu.png",
        BigIconPath: "res://GuZhenRen/images/relics/FeiGuDunGu.png");

    public override decimal ModifyHpLostAfterOstyLate(
        Creature target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner.Creature
            || amount <= 0
            || Counter <= 0
            || props.HasFlag(ValueProp.Unblockable))
        {
            return amount;
        }

        Counter--;
        Flash();
        return 0;
    }
}
