using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Keywords;
using GuZhenRen.Powers;
using GuZhenRen.Tags;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class QuanLiYiFuGu : GuZhenRenCardTemplate
{
    public override int Rank => IsUpgraded ? 6 : 5;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/QuanLiYiFuGu.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.LiDao];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        IsUpgraded
            ? [GuZhenRenKeywords.GaiLv, CardKeyword.Retain]
            : [GuZhenRenKeywords.GaiLv];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<QuanLiYiFuPower>(1)
    ];

    public QuanLiYiFuGu()
        : base(3, CardType.Power, CardRarity.Rare, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (Owner.Creature.GetPower<QuanLiYiFuPower>() is not null)
        {
            return;
        }

        await PowerCmd.Apply<QuanLiYiFuPower>(
            choiceContext,
            Owner.Creature,
            DynamicVars["QuanLiYiFuPower"].BaseValue,
            Owner.Creature,
            this);
    }

    protected override void OnUpgrade()
    {
    }
}
