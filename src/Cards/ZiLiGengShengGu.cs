using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Tags;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class ZiLiGengShengGu : GuZhenRenCardTemplate
{
    private const int BaseHeal = 4;

    public override int Rank => IsUpgraded ? 4 : 3;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/ZiLiGengShengGu.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.LiDao];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        ModCardVars.Computed(
            "CalculatedHeal",
            BaseHeal,
            static card =>
            {
                if (card is null)
                {
                    return BaseHeal;
                }

                var strength = card.Owner?.Creature.GetPowerAmount<StrengthPower>() ?? 0;
                return BaseHeal + strength;
            })
    ];

    public ZiLiGengShengGu()
        : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        var heal = Math.Max(0, DynamicVars.GetComputedValue("CalculatedHeal"));
        if (heal <= 0)
        {
            return;
        }

        await CreatureCmd.Heal(Owner.Creature, heal);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
