using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Tags;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class XueRanZhengPao : AbstractShaZhaoCard
{
    public override bool GainsBlock => true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/XueRanZhengPao.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.XueDao];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("HpLoss", 1),
        ModCardVars.ComputedBlock(
            "Block",
            1,
            static card => card is null
                ? 1
                : Math.Max(
                    1m,
                    card.Owner.Creature.MaxHp
                    - card.Owner.Creature.CurrentHp
                    + 1m))
    ];

    public XueRanZhengPao()
        : base(0, CardType.Skill, CardRarity.Token, TargetType.Self, false)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        var block = DynamicVars.GetComputedValue("Block");

        await CreatureCmd.Damage(
            choiceContext,
            Owner.Creature,
            DynamicVars["HpLoss"].BaseValue,
            ValueProp.Unblockable | ValueProp.Unpowered,
            Owner.Creature,
            this,
            null);

        await CreatureCmd.GainBlock(
            Owner.Creature,
            block,
            ValueProp.Move,
            cardPlay,
            false);
    }
}
