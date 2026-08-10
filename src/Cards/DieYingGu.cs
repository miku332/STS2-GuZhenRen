using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Powers;
using GuZhenRen.Tags;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class DieYingGu : GuZhenRenCardTemplate
{
    public override int Rank => 4;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/DieYingGu.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.JianDao];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(8, ValueProp.Move),
        new PowerVar<JianHenPower>(1).WithPowerTooltip(),
        new DynamicVar("Growth", 2)
    ];

    public DieYingGu()
        : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        var swordShadowCount = PileType.Exhaust
            .GetPile(Owner)
            .Cards
            .Count(card => card is JianYing);
        var growth = DynamicVars["Growth"].BaseValue;
        var damage = DynamicVars.Damage.BaseValue + swordShadowCount * growth;
        var swordMarks = DynamicVars["JianHenPower"].BaseValue + swordShadowCount * growth;

        await DamageCmd.Attack(damage)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        await PowerCmd.Apply<JianHenPower>(
            choiceContext,
            cardPlay.Target,
            swordMarks,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Growth"].UpgradeValueBy(1);
    }
}
