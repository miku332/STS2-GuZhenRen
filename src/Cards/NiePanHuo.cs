using GuZhenRen.CardPools;
using GuZhenRen.Powers;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class NiePanHuo : AbstractShaZhaoCard
{
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/NiePanHuo.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.YanDao];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("HealPercent", 25)
    ];

    public NiePanHuo()
        : base(2, CardType.Skill, CardRarity.Token, TargetType.AllEnemies, false)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(CombatState);

        var hpBefore = Owner.Creature.CurrentHp;
        var damage = Math.Max(0m, hpBefore - 1m);
        if (damage > 0)
        {
            await CreatureCmd.Damage(
                choiceContext,
                Owner.Creature,
                damage,
                ValueProp.Unblockable | ValueProp.Unpowered,
                Owner.Creature,
                this);
        }

        if (!Owner.Creature.IsAlive)
        {
            return;
        }

        var hpLost = Math.Max(0m, hpBefore - Owner.Creature.CurrentHp);
        if (hpLost > 0)
        {
            foreach (var enemy in CombatState.HittableEnemies.ToList())
            {
                if (!enemy.IsAlive)
                {
                    continue;
                }

                await PowerCmd.Apply<FenShaoPower>(
                    choiceContext,
                    enemy,
                    hpLost,
                    Owner.Creature,
                    this);
            }
        }

        var targetHp = Math.Max(
            1m,
            Math.Floor(Owner.Creature.MaxHp
                * DynamicVars["HealPercent"].BaseValue
                / 100m));
        if (Owner.Creature.CurrentHp < targetHp)
        {
            await CreatureCmd.Heal(
                Owner.Creature,
                targetHp - Owner.Creature.CurrentHp);
        }
    }
}
