using System.Collections.Generic;
using System.Threading.Tasks;
using GuZhenRen.CardPools;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.RunData;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Potions;

[RegisterPotion(typeof(GuZhenRenPotionPool))]
public sealed class FuRenXin : ModPotionTemplate
{
    private const int InitialPoison = 6;
    private const int PoisonGrowthPerKill = 3;

    private static readonly PlayerRunSavedData<FuRenXinRunData> SavedData =
        RunSavedDataStore.For(Entry.ModId).RegisterPerPlayer<FuRenXinRunData>(
            "fu-ren-xin",
            static () => new FuRenXinRunData());

    public override PotionRarity Rarity => PotionRarity.Rare;

    public override PotionUsage Usage => PotionUsage.CombatOnly;

    public override TargetType TargetType => TargetType.AnyEnemy;

    public override PotionAssetProfile AssetProfile => new(
        ImagePath: "res://GuZhenRen/images/potions/FuRenXin.png",
        OutlinePath: "res://GuZhenRen/images/potions/FuRenXin_outline.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new PowerVar<PoisonPower>(GetCurrentPoison())
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromPower<PoisonPower>()
    ];

    protected override async Task OnUse(
        PlayerChoiceContext choiceContext,
        Creature? target)
    {
        ArgumentNullException.ThrowIfNull(target);

        await PowerCmd.Apply<PoisonPower>(
            choiceContext,
            target,
            DynamicVars["PoisonPower"].BaseValue,
            Owner.Creature,
            null);
    }

    public static void OnProcured(FuRenXin potion)
    {
        if (!potion.IsMutable || potion.Owner is null)
        {
            return;
        }

        var slot = potion.Owner.GetPotionSlotIndex(potion);
        ClearSlot(potion.Owner, slot);
        RefreshDisplay(potion);
    }

    public static void AfterCreatureDied(CreatureDiedEvent evt)
    {
        if (evt.WasRemovalPrevented
            || evt.CombatState is null
            || evt.Creature.IsSecondaryEnemy
            || !evt.CombatState.Enemies.Contains(evt.Creature)
            || evt.Creature.GetPower<MinionPower>() is not null)
        {
            return;
        }

        foreach (var player in evt.CombatState.Players)
        {
            if (!player.Creature.IsAlive)
            {
                continue;
            }

            foreach (var potion in player.PotionSlots.OfType<FuRenXin>().ToList())
            {
                var slot = player.GetPotionSlotIndex(potion);
                AddBonus(player, slot, PoisonGrowthPerKill);
                RefreshDisplay(potion);
            }
        }
    }

    internal static void ClearSlotBeforeRemoval(Player player, PotionModel potion)
    {
        if (potion is FuRenXin)
        {
            ClearSlot(player, player.GetPotionSlotIndex(potion));
        }
    }

    private static int GetBonus(Player player, int slot)
    {
        return slot < 0 ? 0 : SavedData.Get(player).GetBonus(slot);
    }

    private int GetCurrentPoison()
    {
        if (!IsMutable || Owner is null)
        {
            return InitialPoison;
        }

        return InitialPoison + GetBonus(Owner, Owner.GetPotionSlotIndex(this));
    }

    private static void AddBonus(Player player, int slot, int amount)
    {
        if (slot < 0)
        {
            return;
        }

        SavedData.Modify(player, data => data.SetBonus(slot, data.GetBonus(slot) + amount));
    }

    private static void ClearSlot(Player player, int slot)
    {
        if (slot < 0)
        {
            return;
        }

        SavedData.Modify(player, data => data.SetBonus(slot, 0));
    }

    private static void RefreshDisplay(FuRenXin potion)
    {
        if (!potion.IsMutable || potion.Owner is null)
        {
            return;
        }

        var slot = potion.Owner.GetPotionSlotIndex(potion);
        potion.DynamicVars["PoisonPower"].BaseValue = InitialPoison + GetBonus(potion.Owner, slot);
    }

    public sealed class FuRenXinRunData
    {
        public List<int> SlotBonuses { get; set; } = [];

        public int GetBonus(int slot)
        {
            return slot >= 0 && slot < SlotBonuses.Count
                ? Math.Max(0, SlotBonuses[slot])
                : 0;
        }

        public void SetBonus(int slot, int value)
        {
            if (slot < 0)
            {
                return;
            }

            while (SlotBonuses.Count <= slot)
            {
                SlotBonuses.Add(0);
            }

            SlotBonuses[slot] = Math.Max(0, value);
        }
    }
}
