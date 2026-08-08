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
public sealed class HuoMaoSanZhangGu : GuZhenRenCardTemplate
{
    public override int Rank => IsUpgraded ? 5 : 4;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/HuoMaoSanZhangGu.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.YanDao];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<FenShaoPower>(3)
    ];

    public HuoMaoSanZhangGu()
        : base(2, CardType.Power, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(CombatState);

        if (IsUpgraded)
        {
            foreach (var enemy in CombatState.HittableEnemies.ToList())
            {
                await PowerCmd.Apply<FenShaoPower>(
                    choiceContext,
                    enemy,
                    DynamicVars["FenShaoPower"].BaseValue,
                    Owner.Creature,
                    this);
            }
        }

        if (Owner.Creature.GetPower<HuoMaoSanZhangPower>() is null)
        {
            await PowerCmd.Apply<HuoMaoSanZhangPower>(
                choiceContext,
                Owner.Creature,
                1,
                Owner.Creature,
                this);
        }
    }

    protected override void OnUpgrade()
    {
    }
}
