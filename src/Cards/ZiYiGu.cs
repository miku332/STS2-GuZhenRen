using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Powers;
using GuZhenRen.Tags;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class ZiYiGu : GuZhenRenCardTemplate
{
    public override int Rank => 3;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/ZiYiGu.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.ZhiDao];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<YiPower>(3).WithPowerTooltip(),
        new PowerVar<StrengthPower>(1).WithPowerTooltip(),
        new PowerVar<VulnerablePower>(3).WithPowerTooltip()
    ];

    public ZiYiGu()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.AllEnemies, true)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(CombatState);

        foreach (var enemy in CombatState.HittableEnemies.ToList())
        {
            await PowerCmd.Apply<StrengthPower>(
                choiceContext,
                enemy,
                DynamicVars["StrengthPower"].BaseValue,
                Owner.Creature,
                this);

            await PowerCmd.Apply<VulnerablePower>(
                choiceContext,
                enemy,
                DynamicVars["VulnerablePower"].BaseValue,
                Owner.Creature,
                this);
        }

        await PowerCmd.Apply<YiPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["YiPower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["YiPower"].UpgradeValueBy(2);
        DynamicVars["VulnerablePower"].UpgradeValueBy(2);
    }
}
