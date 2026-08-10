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
using GuZhenRen.Tags;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class DingLi : GuZhenRenCardTemplate
{
    public override int Rank => 6;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/DingLi.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.LiDao];

    public override bool GainsBlock => true;

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Retain
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<StrengthPower>(0).WithPowerTooltip(),
        ModCardVars.ComputedBlock(
            "Block",
            10,
            static card =>
            {
                if (card is null)
                {
                    return 10;
                }

                var strength = card.Owner?.Creature.GetPowerAmount<StrengthPower>() ?? 0;
                var multiplier = card.DynamicVars["StrengthMultiplier"].BaseValue;
                return 10 + strength * multiplier;
            }),
        new DynamicVar("StrengthMultiplier", 3)
    ];

    public DingLi()
        : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(
            Owner.Creature,
            DynamicVars.GetComputedValue("Block"),
            ValueProp.Move,
            cardPlay,
            false);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["StrengthMultiplier"].UpgradeValueBy(2);
    }
}
