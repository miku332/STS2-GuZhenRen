using GuZhenRen.CardPools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class LiDaoDaoHen : ModRelicTemplate
{
    private int _counter = 1;

    public override RelicRarity Rarity => RelicRarity.Event;

    public override bool ShowCounter => true;

    public override int DisplayAmount => Counter;

    [SavedProperty]
    public int Counter
    {
        get => _counter;
        set
        {
            AssertMutable();
            _counter = Math.Max(1, value);
            InvokeDisplayAmountChanged();
        }
    }

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/relics/LiDaoDaoHen.png",
        IconOutlinePath: "res://GuZhenRen/images/relics/outline/LiDaoDaoHen.png",
        BigIconPath: "res://GuZhenRen/images/relics/LiDaoDaoHen.png");

    public override async Task BeforeCombatStart()
    {
        Flash();
        await PowerCmd.Apply<StrengthPower>(
            new ThrowingPlayerChoiceContext(),
            Owner.Creature,
            Counter,
            Owner.Creature,
            null);
    }
}
