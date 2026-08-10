using System.Collections.Generic;
using System.Threading.Tasks;
using GuZhenRen.CardPools;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class HongYunQiTianGu : ModRelicTemplate
{
    private const decimal ProbabilityBonus = 40m;

    public override RelicRarity Rarity => RelicRarity.Rare;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("ProbabilityBonus", ProbabilityBonus)
    ];

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/relics/HongYunQiTianGu.png",
        IconOutlinePath: "res://GuZhenRen/images/relics/outline/HongYunQiTianGu.png",
        BigIconPath: "res://GuZhenRen/images/relics/HongYunQiTianGu.png");

    public override Task AfterObtained()
    {
        Owner.GetRelic<GouShiYun>()?.RefreshChance();
        return Task.CompletedTask;
    }

    public override bool TryModifyCardRewardOptions(
        Player player,
        List<CardCreationResult> cardRewardOptions,
        CardCreationOptions creationOptions)
    {
        if (player != Owner || creationOptions.RarityOdds == CardRarityOddsType.None)
        {
            return false;
        }

        var rareCards = creationOptions.GetPossibleCards(player)
            .Where(card => card.Rarity == CardRarity.Rare)
            .ToList();
        if (rareCards.Count == 0)
        {
            return false;
        }

        var additionalChance = GetAdditionalRareChance(creationOptions.RarityOdds);
        var modified = false;
        foreach (var result in cardRewardOptions)
        {
            if (result.Card.Rarity == CardRarity.Rare
                || Owner.PlayerRng.Rewards.NextFloat() >= (float)additionalChance)
            {
                continue;
            }

            var canonical = Owner.PlayerRng.Rewards.NextItem(rareCards);
            if (canonical is null)
            {
                continue;
            }

            result.ModifyCard(
                player.RunState.CreateCard(canonical, player),
                this);
            modified = true;
        }

        return modified;
    }

    private static decimal GetAdditionalRareChance(CardRarityOddsType oddsType) =>
        oddsType switch
        {
            CardRarityOddsType.RegularEncounter => 0.03m / 0.97m,
            CardRarityOddsType.EliteEncounter => 0.10m / 0.90m,
            CardRarityOddsType.Shop => 0.09m / 0.91m,
            CardRarityOddsType.BossEncounter => 0m,
            CardRarityOddsType.Uniform => 0.33m / 0.67m,
            _ => 0m
        };
}
