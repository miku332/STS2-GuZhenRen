using GuZhenRen.CardPools;
using GuZhenRen.Keywords;
using GuZhenRen.Powers;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class RenGu : AbstractBenMingGuCard
{
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/RenGu.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.JianDao];

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        GuZhenRenKeywords.JianFeng,
        CardKeyword.Exhaust
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Choices", 1),
        new PowerVar<JianFengPower>(0)
    ];

    public RenGu()
        : base(1, CardType.Skill, CardRarity.Token, TargetType.Self)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(CombatState);

        var jianFeng = DynamicVars["JianFengPower"].BaseValue;
        if (jianFeng > 0)
        {
            await PowerCmd.Apply<JianFengPower>(
                choiceContext,
                Owner.Creature,
                jianFeng,
                Owner.Creature,
                this);
        }

        var options = CreateOptions();
        if (options.Count == 0)
        {
            return;
        }

        var selected = options.Count == 1
            ? options[0]
            : await CardSelectCmd.FromChooseACardScreen(
                choiceContext,
                options,
                Owner);
        if (selected is null)
        {
            return;
        }

        await CardPileCmd.AddGeneratedCardToCombat(
            selected,
            PileType.Hand,
            Owner,
            CardPilePosition.Bottom);
    }

    protected override void OnUpgrade()
    {
        SetUpgradedValue("Choices", GetChoices(Rank));
        SetUpgradedValue("JianFengPower", GetJianFeng(Rank));
    }

    private List<CardModel> CreateOptions()
    {
        var rank = Math.Clamp(Rank, 1, 9);
        var upgradeGeneratedCard = rank >= 6;
        var candidates = ModelDb.AllCards
            .OfType<GuZhenRenCardTemplate>()
            .Where(card => card.Id != Id)
            .Where(card => card is not AbstractBenMingGuCard)
            .Where(card => card.Tags.Contains(GuZhenRenTags.JianDao))
            .Where(card => card.Rank is >= 1 and <= 9)
            .Where(card => IsRarityAllowed(card.Rarity, rank))
            .Where(card => !WouldDuplicateXianGu(card, upgradeGeneratedCard))
            .Cast<CardModel>()
            .ToList();

        var rng = Owner.RunState.Rng.CombatCardGeneration;
        for (var i = candidates.Count - 1; i > 0; i--)
        {
            var swapIndex = rng.NextInt(i + 1);
            (candidates[i], candidates[swapIndex]) =
                (candidates[swapIndex], candidates[i]);
        }

        var options = new List<CardModel>();
        var optionCount = Math.Min(
            DynamicVars["Choices"].IntValue,
            candidates.Count);
        foreach (var canonical in candidates.Take(optionCount))
        {
            var card = CombatState!.CreateCard(canonical, Owner);
            if (upgradeGeneratedCard && card.IsUpgradable)
            {
                card.UpgradeInternal();
                card.FinalizeUpgradeInternal();
            }

            card.SetToFreeThisTurn();
            options.Add(card);
        }

        return options;
    }

    private bool WouldDuplicateXianGu(
        GuZhenRenCardTemplate candidate,
        bool upgradeGeneratedCard)
    {
        var generatedRank = candidate.Rank
            + (upgradeGeneratedCard && candidate.IsUpgradable ? 1 : 0);
        if (generatedRank < 6)
        {
            return false;
        }

        return Owner.Deck.Cards.Any(card =>
            card.Id == candidate.Id
            && card is GuZhenRenCardTemplate guCard
            && guCard.Rank >= 6);
    }

    private static bool IsRarityAllowed(CardRarity rarity, int rank) =>
        rank switch
        {
            <= 2 => rarity == CardRarity.Common,
            3 => rarity is CardRarity.Common or CardRarity.Uncommon,
            _ => rarity is CardRarity.Common or CardRarity.Uncommon or CardRarity.Rare
        };

    private static int GetChoices(int rank) =>
        rank switch
        {
            1 => 1,
            <= 7 => 2,
            _ => 3
        };

    private static int GetJianFeng(int rank) =>
        rank switch
        {
            5 or 6 => 1,
            7 or 8 => 2,
            >= 9 => 3,
            _ => 0
        };

    private void SetUpgradedValue(string name, decimal targetValue)
    {
        var dynamicVar = DynamicVars[name];
        dynamicVar.UpgradeValueBy(targetValue - dynamicVar.BaseValue);
    }
}
