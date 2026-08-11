using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Tags;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class WuZhiQuanXinJian : AbstractShaZhaoCard
{
    private const int InitialUses = 5;

    private int _remainingUses = InitialUses;

    public override int Rank => 0;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/WuZhiQuanXinJian.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.JianDao];

    [SavedProperty]
    public int RemainingUses
    {
        get => _remainingUses;
        set
        {
            AssertMutable();
            _remainingUses = Math.Clamp(value, 0, InitialUses);
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(9, ValueProp.Move),
        new DynamicVar("Multiplier", 3),
        new DynamicVar("RemainingUses", RemainingUses)
    ];

    public WuZhiQuanXinJian()
        : base(1, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy, false)
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

        DynamicVars.Damage.BaseValue *= DynamicVars["Multiplier"].BaseValue;

        if (cardPlay.PlayIndex == cardPlay.PlayCount - 1)
        {
            RemainingUses--;
            DynamicVars["RemainingUses"].BaseValue = RemainingUses;
        }
    }

    protected override PileType GetResultPileTypeForCardPlay() =>
        RemainingUses <= 1
            ? PileType.Exhaust
            : base.GetResultPileTypeForCardPlay();
}
