using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Powers;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class LongLinGu : GuZhenRenCardTemplate
{
    private const int BaseBlock = 2;
    private const int BaseTimes = 3;

    public override int Rank => IsUpgraded ? 8 : 7;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/LongLinGu.png");

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new BlockVar(BaseBlock, ValueProp.Move),
        new CalculationBaseVar(BaseTimes),
        new CalculationExtraVar(1),
        new CalculatedVar("CalculatedTimes")
            .WithMultiplier(static (CardModel card, Creature? _) =>
                card.Owner?.Creature.GetPowerAmount<JianFengPower>() ?? 0)
    ];

    public LongLinGu()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        var times = Math.Max(
            0,
            (int)((CalculatedVar)DynamicVars["CalculatedTimes"])
                .Calculate(Owner.Creature));

        for (var i = 0; i < times; i++)
        {
            await CreatureCmd.GainBlock(
                Owner.Creature,
                DynamicVars.Block.BaseValue,
                DynamicVars.Block.Props,
                cardPlay,
                false);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.CalculationBase.UpgradeValueBy(1);
    }
}
