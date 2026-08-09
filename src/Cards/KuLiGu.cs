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
public sealed class KuLiGu : GuZhenRenCardTemplate
{
    public override int Rank => 4;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/KuLiGu.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.LiDao];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Threshold", 6),
        ModCardVars.Computed(
            "CalculatedStrength",
            0,
            static card =>
            {
                if (card?.Owner?.Creature is null)
                {
                    return 0;
                }

                var threshold = card.DynamicVars["Threshold"].BaseValue;
                if (threshold <= 0)
                {
                    return 0;
                }

                var owner = card.Owner.Creature;
                var missingHp = Math.Max(0, owner.MaxHp - owner.CurrentHp);
                return Math.Floor(missingHp / threshold);
            })
    ];

    public KuLiGu()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        var amount = DynamicVars.GetComputedValue("CalculatedStrength");
        if (amount <= 0)
        {
            return;
        }

        await PowerCmd.Apply<KuLiGuStrengthPower>(
            choiceContext,
            Owner.Creature,
            amount,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Threshold"].UpgradeValueBy(-2);
    }
}
