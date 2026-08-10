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
public sealed class TianPuGuangHe : AbstractShaZhaoCard
{
    public override int Rank => 0;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/TianPuGuangHe.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.GuangDao];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<TaiGuRongYaoZhiGuangPower>(3).WithPowerTooltip()
    ];

    public TianPuGuangHe()
        : base(1, CardType.Skill, CardRarity.Token, TargetType.AllEnemies, false)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(CombatState);

        foreach (var enemy in CombatState.HittableEnemies.ToList())
        {
            await PowerCmd.Apply<TaiGuRongYaoZhiGuangPower>(
                choiceContext,
                enemy,
                DynamicVars["TaiGuRongYaoZhiGuangPower"].BaseValue,
                Owner.Creature,
                this);
        }
    }
}
