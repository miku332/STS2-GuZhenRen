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
public sealed class LiaoYuanHuo : GuZhenRenCardTemplate
{
    public override int Rank => IsUpgraded ? 6 : 5;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/LiaoYuanHuo.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.YanDao];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Times", 2),
        new PowerVar<FenShaoPower>(3).WithPowerTooltip()
    ];

    public LiaoYuanHuo()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(CombatState);

        var times = (int)DynamicVars["Times"].BaseValue;
        for (var i = 0; i < times; i++)
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

        var burn = Owner.Creature.CombatState!.CreateCard<Burn>(Owner);
        await CardPileCmd.AddGeneratedCardToCombat(
            burn,
            PileType.Hand,
            Owner,
            CardPilePosition.Bottom);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Times"].UpgradeValueBy(1);
        DynamicVars["FenShaoPower"].UpgradeValueBy(-1);
    }
}
