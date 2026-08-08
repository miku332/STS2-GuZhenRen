using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Powers;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class ZhanNianGu : GuZhenRenCardTemplate
{
    public override int Rank => 2;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/ZhanNianGu.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(9, ValueProp.Move),
        new PowerVar<NianPower>(4)
    ];

    public ZhanNianGu()
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

        if (WasPreviousCardAttack())
        {
            await PowerCmd.Apply<NianPower>(
                choiceContext,
                Owner.Creature,
                DynamicVars["NianPower"].BaseValue,
                Owner.Creature,
                this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars["NianPower"].UpgradeValueBy(1);
    }

    private bool WasPreviousCardAttack()
    {
        if (CombatState is null)
        {
            return false;
        }

        CardModel? previousCard = null;
        foreach (CardPlayFinishedEntry entry in CombatManager.Instance.History.CardPlaysFinished)
        {
            if (entry.HappenedThisTurn(CombatState)
                && entry.CardPlay.Card.Owner == Owner)
            {
                previousCard = entry.CardPlay.Card;
            }
        }

        return previousCard?.Type == CardType.Attack;
    }
}
