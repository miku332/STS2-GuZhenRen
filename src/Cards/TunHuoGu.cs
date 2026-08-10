using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Powers;
using GuZhenRen.Tags;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class TunHuoGu : GuZhenRenCardTemplate
{
    public override int Rank => IsUpgraded ? 5 : 4;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/TunHuoGu.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.YanDao];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<TunHuoPower>(1).WithPowerTooltip(),
        new CardsVar(2)
    ];

    public TunHuoGu()
        : base(2, CardType.Power, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await PowerCmd.Apply<TunHuoPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["TunHuoPower"].BaseValue,
            Owner.Creature,
            this);

        if (!IsUpgraded || CombatState is null)
        {
            return;
        }

        for (var i = 0; i < DynamicVars["Cards"].BaseValue; i++)
        {
            var huoShi = CombatState.CreateCard<HuoShi>(Owner);
            await CardPileCmd.AddGeneratedCardToCombat(
                huoShi,
                PileType.Hand,
                Owner,
                CardPilePosition.Bottom);
        }
    }

    protected override void OnUpgrade()
    {
    }
}
