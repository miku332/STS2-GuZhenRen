using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Powers;
using GuZhenRen.Tags;
using GuZhenRen.Keywords;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class ZhiHuiGu : AbstractBenMingGuCard
{
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/ZhiHuiGu.png");

    public override IEnumerable<CardKeyword> CanonicalKeywords
    {
        get
        {
            yield return GuZhenRenKeywords.Nian;

            if (Rank >= 9)
            {
                yield return CardKeyword.Innate;
            }
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<ZhiHuiPower>(2)
    ];

    public ZhiHuiGu()
        : base(1, CardType.Power, CardRarity.Token, TargetType.Self)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await PowerCmd.Apply<ZhiHuiPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["ZhiHuiPower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["ZhiHuiPower"].UpgradeValueBy(
            GetNianAmount(Rank) - DynamicVars["ZhiHuiPower"].BaseValue);

        if (Rank >= 9)
        {
            AddKeyword(CardKeyword.Innate);
        }
        else
        {
            RemoveKeyword(CardKeyword.Innate);
        }
    }

    private static int GetNianAmount(int rank) =>
        Math.Min(rank + 1, 9);
}
