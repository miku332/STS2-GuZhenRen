using GuZhenRen.CardPools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class GuCiGu : ModRelicTemplate
{
    private const int HpLoss = 1;
    private const int ThornsAmount = 4;

    public override RelicRarity Rarity => RelicRarity.Event;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/relics/GuCiGu.png",
        IconOutlinePath: "res://GuZhenRen/images/relics/outline/GuCiGu.png",
        BigIconPath: "res://GuZhenRen/images/relics/GuCiGu.png");

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [HoverTipFactory.FromPower<ThornsPower>(ThornsAmount)];

    public override async Task BeforeCombatStart()
    {
        Flash();
        var choiceContext = new ThrowingPlayerChoiceContext();
        await CreatureCmd.Damage(
            choiceContext,
            Owner.Creature,
            HpLoss,
            ValueProp.Unblockable | ValueProp.Unpowered,
            Owner.Creature,
            null);

        if (!Owner.Creature.IsAlive)
        {
            return;
        }

        await PowerCmd.Apply<ThornsPower>(
            choiceContext,
            Owner.Creature,
            ThornsAmount,
            Owner.Creature,
            null);
    }
}
