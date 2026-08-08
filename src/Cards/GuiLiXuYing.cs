using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class GuiLiXuYing : AbstractXuYingCard
{
    private const int BaseBlock = 3;

    protected override int ChancePercent => 30;

    protected override bool RequiresLiveTarget => false;

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> AdditionalVars =>
    [
        ModCardVars.ComputedBlock(
            "CalculatedBlock",
            BaseBlock,
            static card =>
            {
                if (card is null)
                {
                    return BaseBlock;
                }

                var strength = card.Owner?.Creature.GetPowerAmount<StrengthPower>() ?? 0;
                return card.DynamicVars["BlockBase"].BaseValue + strength;
            }),
        new DynamicVar("BlockBase", BaseBlock)
    ];

    public override int Rank => 3;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/GuiLiXuYing.png");

    public GuiLiXuYing()
        : base(CardType.Skill, TargetType.Self)
    {
    }

    protected override async Task TriggerXuYingEffect(
        PlayerChoiceContext choiceContext,
        CardPlay triggerCardPlay)
    {
        var block = Math.Max(0, DynamicVars.GetComputedValue("CalculatedBlock"));

        await CreatureCmd.GainBlock(
            Owner.Creature,
            block,
            ValueProp.Move,
            null,
            false);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Chance"].UpgradeValueBy(10);
        DynamicVars["BlockBase"].UpgradeValueBy(1);
    }
}
