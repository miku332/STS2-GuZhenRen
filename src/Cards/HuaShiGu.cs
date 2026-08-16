using GuZhenRen.CardPools;
using GuZhenRen.Enchantments;
using GuZhenRen.Keywords;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class HuaShiGu : GuZhenRenCardTemplate
{
    public override int Rank => IsUpgraded ? 6 : 5;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/HuaShiGu.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.TuDao];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [GuZhenRenKeywords.HuaShi];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("HuaShi", 4)
    ];

    public HuaShiGu()
        : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self, true)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        var hand = PileType.Hand.GetPile(Owner);
        var enchantment = ModelDb.Enchantment<HuaShiEnchantment>();
        if (!hand.Cards.Any(enchantment.CanEnchant))
        {
            return;
        }

        var selectedCards = await CardSelectCmd.FromHand(
            choiceContext,
            Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 1),
            enchantment.CanEnchant,
            this);
        var selected = selectedCards.FirstOrDefault();
        if (selected is null)
        {
            return;
        }

        CardCmd.Enchant<HuaShiEnchantment>(
            selected,
            DynamicVars["HuaShi"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["HuaShi"].UpgradeValueBy(2);
    }
}
