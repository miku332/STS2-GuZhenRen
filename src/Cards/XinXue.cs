using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Powers;
using GuZhenRen.Tags;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class XinXue : GuZhenRenCardTemplate
{
    public override int Rank => 1;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/XinXue.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.XueDao];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("HealAmount", 2),
        new PowerVar<XinXuePower>(1)
    ];

    public XinXue()
        : base(1, CardType.Power, CardRarity.Rare, TargetType.Self, false)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await CreatureCmd.Heal(
            Owner.Creature,
            DynamicVars["HealAmount"].BaseValue);

        await PowerCmd.Apply<XinXuePower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["XinXuePower"].BaseValue,
            Owner.Creature,
            this);
    }
}
