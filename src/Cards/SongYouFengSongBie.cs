using GuZhenRen.CardPools;
using GuZhenRen.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class SongYouFengSongBie : GuZhenRenCardTemplate
{
    public override int Rank => 0;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/SongYouFengSongBie.png");

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Exhaust,
        CardKeyword.Ethereal
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<SongYouFadingPower>(2)
    ];

    public SongYouFengSongBie()
        : base(1, CardType.Skill, CardRarity.Token, TargetType.AnyEnemy, false)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);

        var friend = cardPlay.Target.GetPower<HaoYouPower>();
        if (friend is null)
        {
            return;
        }

        if (cardPlay.Target.GetPower<SongYouFadingPower>() is null)
        {
            await PowerCmd.Apply<SongYouFadingPower>(
                choiceContext,
                cardPlay.Target,
                DynamicVars["SongYouFadingPower"].BaseValue,
                Owner.Creature,
                this);
        }

        await PowerCmd.Remove(friend);
    }
}
