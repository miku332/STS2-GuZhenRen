using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Powers;
using GuZhenRen.Tags;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class XinXue : AbstractBenMingGuCard
{
    protected override int MaxRank => 8;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/XinXue.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.XueDao];

    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            if (Rank >= 6)
            {
                yield return CardKeyword.Innate;
            }
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("HealAmount", 2),
        new PowerVar<XinXuePower>(1)
    ];

    public XinXue()
        : base(1, CardType.Power, CardRarity.Token, TargetType.Self)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await CreatureCmd.Heal(
            Owner.Creature,
            DynamicVars["HealAmount"].BaseValue);

        await PowerCmd.Apply<XinXuePower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["XinXuePower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["HealAmount"].UpgradeValueBy(
            GetHealAmount(Rank) - DynamicVars["HealAmount"].BaseValue);
        DynamicVars["XinXuePower"].UpgradeValueBy(
            GetMultiplier(Rank) - DynamicVars["XinXuePower"].BaseValue);

        if (Rank >= 6)
        {
            AddKeyword(CardKeyword.Innate);
        }
        else
        {
            RemoveKeyword(CardKeyword.Innate);
        }
    }

    private static int GetHealAmount(int rank) =>
        rank switch
        {
            1 or 2 => 2,
            3 or 4 => 3,
            5 or 6 => 4,
            _ => 5
        };

    private static int GetMultiplier(int rank) =>
        rank switch
        {
            1 => 1,
            2 or 3 => 2,
            4 or 5 => 3,
            6 or 7 => 4,
            _ => 5
        };
}
