using System.Collections.Generic;
using GuZhenRen.CardPools;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class FengXiongHuaJi : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/relics/FengXiongHuaJi.png",
        IconOutlinePath: "res://GuZhenRen/images/relics/outline/FengXiongHuaJi.png",
        BigIconPath: "res://GuZhenRen/images/relics/FengXiongHuaJi.png");

    public override bool TryModifyCardBeingAddedToDeck(
        CardModel card,
        out CardModel? newCard)
    {
        newCard = null;
        if (card.Owner != Owner || card.Type != CardType.Curse)
        {
            return false;
        }

        var candidates = Owner.Character.CardPool
            .GetUnlockedCards(
                Owner.UnlockState,
                Owner.RunState.CardMultiplayerConstraint)
            .Where(candidate => candidate.Rarity is
                CardRarity.Common or CardRarity.Uncommon or CardRarity.Rare)
            .ToList();
        if (candidates.Count == 0)
        {
            return false;
        }

        var canonical = Owner.RunState.Rng.UpFront.NextItem(candidates);
        if (canonical is null)
        {
            return false;
        }

        newCard = Owner.RunState.CreateCard(canonical, Owner);
        newCard.FloorAddedToDeck = card.FloorAddedToDeck;
        Flash();
        return true;
    }
}
