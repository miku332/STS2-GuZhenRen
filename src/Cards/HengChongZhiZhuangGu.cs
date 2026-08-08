using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Tags;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class HengChongZhiZhuangGu : GuZhenRenCardTemplate
{
    public override int Rank => IsUpgraded ? 5 : 4;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/HengChongZhiZhuangGu.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.LiDao];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(7, ValueProp.Move),
        new DynamicVar("Hits", 2),
        new DynamicVar("SelfDamage", 2)
    ];

    public HengChongZhiZhuangGu()
        : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        var hits = (int)DynamicVars["Hits"].BaseValue;
        for (var i = 0; i < hits; i++)
        {
            if (!cardPlay.Target.IsAlive)
            {
                break;
            }

            var attack = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this)
                .Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_blunt")
                .Execute(choiceContext);

            var wasFullyBlocked = attack.Results
                .SelectMany(static resultSet => resultSet)
                .Any(result => result.Receiver == cardPlay.Target
                    && result.WasFullyBlocked
                    && result.TotalDamage > 0);

            if (wasFullyBlocked)
            {
                await CreatureCmd.Damage(
                    choiceContext,
                    Owner.Creature,
                    DynamicVars["SelfDamage"].BaseValue,
                    ValueProp.Move,
                    Owner.Creature,
                    this);
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
    }
}
