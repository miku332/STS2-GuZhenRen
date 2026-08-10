using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Powers;
using GuZhenRen.Tags;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class JianYingGu : GuZhenRenCardTemplate
{
    public override int Rank => 2;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/JianYingGu.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.JianDao];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(4, ValueProp.Move),
        new PowerVar<JianHenPower>(2).WithPowerTooltip()
    ];

    protected override IEnumerable<GeneratedCardPreview> GeneratedCardPreviews =>
        [PreviewCard<JianYing>()];

    public JianYingGu()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        await PowerCmd.Apply<JianHenPower>(
            choiceContext,
            cardPlay.Target,
            DynamicVars["JianHenPower"].BaseValue,
            Owner.Creature,
            this);

        var swordShadow = Owner.Creature.CombatState!.CreateCard<JianYing>(Owner);
        await CardPileCmd.AddGeneratedCardToCombat(
            swordShadow,
            PileType.Hand,
            Owner,
            CardPilePosition.Bottom);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["JianHenPower"].UpgradeValueBy(3);
    }
}
