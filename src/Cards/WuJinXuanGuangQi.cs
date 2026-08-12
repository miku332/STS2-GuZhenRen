using GuZhenRen.CardPools;
using GuZhenRen.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class WuJinXuanGuangQi : AbstractShaZhaoCard
{
    public override int Rank => 0;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/WuJinXuanGuangQi.png");

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<SlowPower>(1).WithPowerTooltip(),
        new PowerVar<WuJinXuanGuangQiPower>(1),
        new PowerVar<StrengthPower>(0).WithPowerTooltip()
    ];

    public WuJinXuanGuangQi()
        : base(2, CardType.Skill, CardRarity.Token, TargetType.AllEnemies, false)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(CombatState);

        foreach (var enemy in CombatState.HittableEnemies.ToList())
        {
            if (enemy.GetPower<SlowPower>() is null)
            {
                await PowerCmd.Apply<SlowPower>(
                    choiceContext,
                    enemy,
                    DynamicVars["SlowPower"].BaseValue,
                    Owner.Creature,
                    this);
            }

            await PowerCmd.Apply<WuJinXuanGuangQiPower>(
                choiceContext,
                enemy,
                DynamicVars["WuJinXuanGuangQiPower"].BaseValue,
                Owner.Creature,
                this);
        }
    }

    protected override void OnUpgrade()
    {
    }
}
