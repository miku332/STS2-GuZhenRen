using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Powers;
using GuZhenRen.Tags;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class LiLiangGu : GuZhenRenCardTemplate
{
    public override int Rank => IsUpgraded ? 2 : 1;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/LiLiangGu.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.LiDao];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<StrengthPower>(2),
        new PowerVar<LiLiangGuStrengthDownPower>(1)
    ];

    public LiLiangGu()
        : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        var totalStrengthGain = DynamicVars["StrengthPower"].BaseValue;
        var temporaryStrength = DynamicVars["LiLiangGuStrengthDownPower"].BaseValue;
        var permanentStrength = totalStrengthGain - temporaryStrength;

        if (permanentStrength > 0)
        {
            await PowerCmd.Apply<StrengthPower>(
                choiceContext,
                Owner.Creature,
                permanentStrength,
                Owner.Creature,
                this);
        }

        if (temporaryStrength > 0)
        {
            await PowerCmd.Apply<LiLiangGuStrengthDownPower>(
                choiceContext,
                Owner.Creature,
                temporaryStrength,
                Owner.Creature,
                this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["LiLiangGuStrengthDownPower"].UpgradeValueBy(-1);
    }
}
