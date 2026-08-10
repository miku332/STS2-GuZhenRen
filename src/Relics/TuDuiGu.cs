using GuZhenRen.CardPools;
using GuZhenRen.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class TuDuiGu : ModRelicTemplate
{
    private const int JiTuAmount = 3;

    public override RelicRarity Rarity => RelicRarity.Common;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/relics/TuDuiGu.png",
        IconOutlinePath: "res://GuZhenRen/images/relics/outline/TuDuiGu.png",
        BigIconPath: "res://GuZhenRen/images/relics/TuDuiGu.png");

    public override async Task BeforeCombatStart()
    {
        Flash();
        await PowerCmd.Apply<JiTuPower>(
            new ThrowingPlayerChoiceContext(),
            Owner.Creature,
            JiTuAmount,
            Owner.Creature,
            null);
    }
}
