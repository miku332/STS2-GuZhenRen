using GuZhenRen.CardPools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class TieGuGu : ModRelicTemplate
{
    private const int MaxHpLoss = 3;
    private const int MetallicizeAmount = 3;

    public override RelicRarity Rarity => RelicRarity.Event;

    public override bool HasUponPickupEffect => true;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/relics/TieGuGu.png",
        IconOutlinePath: "res://GuZhenRen/images/relics/outline/TieGuGu.png",
        BigIconPath: "res://GuZhenRen/images/relics/TieGuGu.png");

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [HoverTipFactory.FromPower<PlatingPower>(MetallicizeAmount)];

    public override async Task AfterObtained()
    {
        await CreatureCmd.LoseMaxHp(
            new ThrowingPlayerChoiceContext(),
            Owner.Creature,
            MaxHpLoss,
            false);
    }

    public override async Task BeforeCombatStart()
    {
        Flash();
        await PowerCmd.Apply<PlatingPower>(
            new ThrowingPlayerChoiceContext(),
            Owner.Creature,
            MetallicizeAmount,
            Owner.Creature,
            null);
    }
}
