using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Powers;
using GuZhenRen.Tags;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class LiLiangGu : AbstractBenMingGuCard
{
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/LiLiangGu.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.LiDao];

    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            yield return CardKeyword.Exhaust;
            if (Rank >= 8)
            {
                yield return CardKeyword.Innate;
            }
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<StrengthPower>(2).WithPowerTooltip(),
        new PowerVar<LiLiangGuStrengthDownPower>(1)
    ];

    public LiLiangGu()
        : base(0, CardType.Skill, CardRarity.Token, TargetType.Self)
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
        SetUpgradedValue("StrengthPower", GetStrength(Rank));
        SetUpgradedValue(
            "LiLiangGuStrengthDownPower",
            GetTemporaryStrength(Rank));

        if (Rank >= 8)
        {
            AddKeyword(CardKeyword.Innate);
        }
        else
        {
            RemoveKeyword(CardKeyword.Innate);
        }
    }

    private static int GetStrength(int rank) =>
        rank switch
        {
            <= 2 => 2,
            <= 4 => 3,
            5 => 4,
            6 => 5,
            7 => 6,
            8 => 7,
            _ => 8
        };

    private static int GetTemporaryStrength(int rank) =>
        rank is 1 or 3 ? 1 : 0;

    private void SetUpgradedValue(string name, decimal targetValue)
    {
        var dynamicVar = DynamicVars[name];
        dynamicVar.UpgradeValueBy(targetValue - dynamicVar.BaseValue);
    }
}
