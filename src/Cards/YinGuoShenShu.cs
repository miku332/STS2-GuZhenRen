using GuZhenRen.CardPools;
using GuZhenRen.Powers;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class YinGuoShenShu : AbstractShaZhaoCard
{
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/YinGuoShenShu.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.MuDao];

    protected override IEnumerable<GeneratedCardPreview> GeneratedCardPreviews =>
        [PreviewCard<LaiYinQuGuo>()];

    public YinGuoShenShu()
        : base(3, CardType.Power, CardRarity.Token, TargetType.Self, false)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await PowerCmd.Apply<YinPower>(
            choiceContext,
            Owner.Creature,
            1,
            Owner.Creature,
            this);

        ArgumentNullException.ThrowIfNull(CombatState);
        var card = CombatState.CreateCard<LaiYinQuGuo>(Owner);
        await CardPileCmd.AddGeneratedCardToCombat(
            card,
            PileType.Discard,
            Owner,
            CardPilePosition.Bottom);
    }
}
