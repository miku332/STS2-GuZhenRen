using GuZhenRen.CardPools;
using GuZhenRen.Keywords;
using GuZhenRen.Powers;
using GuZhenRen.Systems;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class LuanFangHunXiangWu : AbstractShaZhaoCard, IProbabilityCard
{
    private const decimal InitialChance = 50m;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/LuanFangHunXiangWu.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.ZhiDao];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust,
        GuZhenRenKeywords.GaiLv
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new ProbabilityVar("Chance", InitialChance)
    ];

    public LuanFangHunXiangWu()
        : base(2, CardType.Skill, CardRarity.Token, TargetType.AllEnemies, false)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(CombatState);

        var chance = ProbabilitySystem.GetEffectiveChance(
            this,
            DynamicVars["Chance"].BaseValue);

        foreach (var enemy in CombatState.HittableEnemies.ToList())
        {
            if (!enemy.IsAlive)
            {
                continue;
            }

            await CreatureCmd.Stun(enemy);
            if (chance > 0)
            {
                await PowerCmd.Apply<YunTouZhuanXiangPower>(
                    choiceContext,
                    enemy,
                    chance,
                    Owner.Creature,
                    this);
            }
        }
    }

    public void IncreaseBaseChance(decimal percentagePoints)
    {
        var chance = DynamicVars["Chance"];
        chance.BaseValue = Math.Clamp(
            chance.BaseValue + percentagePoints,
            0m,
            100m);
    }
}
