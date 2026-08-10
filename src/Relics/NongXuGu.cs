using GuZhenRen.CardPools;
using GuZhenRen.Cards;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class NongXuGu : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/relics/NongXuGu.png",
        IconOutlinePath: "res://GuZhenRen/images/relics/outline/NongXuGu.png",
        BigIconPath: "res://GuZhenRen/images/relics/NongXuGu.png");

    public override async Task BeforeCombatStart()
    {
        var combatState = Owner.Creature.CombatState;
        if (combatState is null)
        {
            return;
        }

        var candidates = ModelDb.AllCards
            .OfType<AbstractXuYingCard>()
            .Cast<CardModel>()
            .ToList();
        var canonical = Owner.RunState.Rng.CombatCardGeneration
            .NextItem(candidates);
        if (canonical is null)
        {
            return;
        }

        Flash();
        var card = combatState.CreateCard(canonical, Owner);
        await CardPileCmd.AddGeneratedCardToCombat(
            card,
            PileType.Hand,
            Owner,
            CardPilePosition.Bottom);
    }
}
