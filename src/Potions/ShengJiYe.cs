using GuZhenRen.CardPools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Potions;

[RegisterPotion(typeof(GuZhenRenPotionPool))]
public sealed class ShengJiYe : ModPotionTemplate
{
    public override PotionRarity Rarity => PotionRarity.Token;

    public override PotionUsage Usage => PotionUsage.AnyTime;

    public override TargetType TargetType => TargetType.AnyPlayer;

    public override PotionAssetProfile AssetProfile => new(
        ImagePath: "res://GuZhenRen/images/potions/ShengJiYe.png",
        OutlinePath: "res://GuZhenRen/images/potions/ShengJiYe_outline.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Heal", 6)
    ];

    protected override async Task OnUse(
        PlayerChoiceContext choiceContext,
        Creature? target)
    {
        await CreatureCmd.Heal(
            target ?? Owner.Creature,
            DynamicVars["Heal"].BaseValue);
    }
}
