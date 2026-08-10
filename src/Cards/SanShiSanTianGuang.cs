using GuZhenRen.CardPools;
using GuZhenRen.Keywords;
using GuZhenRen.Powers;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class SanShiSanTianGuang : AbstractShaZhaoCard
{
    private int _pendingShanYaoGain;

    public override int Rank => 0;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/SanShiSanTianGuang.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.GuangDao];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<ShanYaoPower>(0).WithPowerTooltip(),
        new DamageVar(4, ValueProp.Move),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("ShanYaoGained")
            .WithMultiplier(static (CardModel card, Creature? _) =>
                card.Owner.Creature.GetPowerAmount<ShanYaoHistoryPower>())
    ];

    public SanShiSanTianGuang()
        : base(2, CardType.Attack, CardRarity.Token, TargetType.AllEnemies, false)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(CombatState);

        _pendingShanYaoGain = Owner.Creature
            .GetPowerAmount<ShanYaoHistoryPower>();

        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .TargetingAllOpponents(CombatState)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
    }

    public override async Task AfterCardPlayedLate(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (cardPlay.Card != this || _pendingShanYaoGain <= 0)
        {
            return;
        }

        var amountToGain = _pendingShanYaoGain;
        _pendingShanYaoGain = 0;
        await ShanYaoPower.Apply(
            choiceContext,
            Owner.Creature,
            amountToGain,
            Owner.Creature,
            this);
    }
}
