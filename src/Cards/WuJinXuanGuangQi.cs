using GuZhenRen.CardPools;
using GuZhenRen.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class WuJinXuanGuangQi : GuZhenRenCardTemplate
{
    public override int Rank => 0;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/WuJinXuanGuangQi.png");

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
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
                    1,
                    Owner.Creature,
                    this);
            }

            await PowerCmd.Apply<WuJinXuanGuangQiPower>(
                choiceContext,
                enemy,
                1,
                Owner.Creature,
                this);
        }
    }

    protected override void OnUpgrade()
    {
    }
}
