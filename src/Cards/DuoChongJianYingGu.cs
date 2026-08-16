using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using GuZhenRen.CardPools;
using GuZhenRen.Tags;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class DuoChongJianYingGu : GuZhenRenCardTemplate
{
    public override int Rank => IsUpgraded ? 5 : 4;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/DuoChongJianYingGu.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.JianDao];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(3)
    ];

    protected override IEnumerable<GeneratedCardPreview> GeneratedCardPreviews =>
        [PreviewCard<JianYing>(IsUpgraded)];

    public DuoChongJianYingGu()
        : base(1, CardType.Skill, CardRarity.Common, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(CombatState);

        for (var i = 0; i < (int)DynamicVars["Cards"].BaseValue; i++)
        {
            var swordShadow = CombatState.CreateCard<JianYing>(Owner);
            if (IsUpgraded)
            {
                swordShadow.UpgradeInternal();
                swordShadow.FinalizeUpgradeInternal();
            }

            await CardPileCmd.AddGeneratedCardToCombat(
                swordShadow,
                PileType.Hand,
                Owner,
                CardPilePosition.Bottom);
        }
    }

    protected override void OnUpgrade()
    {
    }
}
