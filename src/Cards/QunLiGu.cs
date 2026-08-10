using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Keywords;
using GuZhenRen.Tags;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class QunLiGu : GuZhenRenCardTemplate
{
    private const int BaseDamage = 8;

    public override int Rank => IsUpgraded ? 6 : 5;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/QunLiGu.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.LiDao];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [GuZhenRenKeywords.XuYing];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(BaseDamage, ValueProp.Move),
        new DynamicVar("ExtraDamage", 8),
        ModCardVars.ComputedDamage(
            "CalculatedDamage",
            BaseDamage,
            static (card, _) => card is QunLiGu qunLiGu
                ? qunLiGu.CalculateCurrentDamage()
                : BaseDamage,
            ValueProp.Move)
    ];

    public QunLiGu()
        : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        await DamageCmd.Attack(DynamicVars.GetComputedValue("CalculatedDamage"))
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars["ExtraDamage"].UpgradeValueBy(2);
    }

    private decimal CalculateCurrentDamage()
    {
        var shadows = PileType.Hand.GetPile(Owner)
            .Cards
            .Count(card => card.Tags.Contains(GuZhenRenTags.XuYing));

        return DynamicVars.Damage.BaseValue
            + shadows * DynamicVars["ExtraDamage"].BaseValue;
    }
}
