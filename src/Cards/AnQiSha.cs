using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
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
public sealed class AnQiSha : GuZhenRenCardTemplate
{
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/AnQiSha.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.JianDao];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(10, ValueProp.Move)
    ];

    public AnQiSha()
        : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy, false)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        var attackCount = CombatManager.Instance.History.CardPlaysStarted.Count(entry =>
            entry.HappenedThisTurn(CombatState)
            && entry.Actor == Owner.Creature
            && entry.CardPlay.Card.Type == CardType.Attack);
        var targetIsAttacking = cardPlay.Target.Monster?.IntendsToAttack == true;
        var damage = DynamicVars.Damage.BaseValue;

        if (attackCount == 1 && !targetIsAttacking)
        {
            damage *= 10;
        }

        await DamageCmd.Attack(damage)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }
}
