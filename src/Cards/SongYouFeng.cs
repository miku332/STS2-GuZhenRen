using GuZhenRen.CardPools;
using GuZhenRen.Powers;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class SongYouFeng : GuZhenRenCardTemplate
{
    public override int Rank => 0;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/SongYouFeng.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.FengDao];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<HaoYouPower>(1).WithPowerTooltip()];

    public SongYouFeng()
        : base(1, CardType.Skill, CardRarity.Token, TargetType.AnyEnemy, false)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        ArgumentNullException.ThrowIfNull(CombatState);

        var isAlreadyFriend = cardPlay.Target.GetPower<HaoYouPower>() is not null;
        var farewell = CombatState.CreateCard<SongYouFengSongBie>(Owner);
        await CardPileCmd.AddGeneratedCardToCombat(
            farewell,
            isAlreadyFriend ? PileType.Hand : PileType.Discard,
            Owner,
            CardPilePosition.Bottom);

        await PowerCmd.Apply<HaoYouPower>(
            choiceContext,
            cardPlay.Target,
            DynamicVars["HaoYouPower"].BaseValue,
            Owner.Creature,
            this);
    }
}
