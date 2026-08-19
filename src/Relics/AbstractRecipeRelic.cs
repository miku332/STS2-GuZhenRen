using GuZhenRen.RestSite;
using GuZhenRen.Systems;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interactions.RightClick;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Relics;

public abstract class AbstractRecipeRelic
    : ModRelicTemplate, IModRightClickableRelic
{
    private bool _isCrafted;

    public override RelicRarity Rarity => RelicRarity.Event;

    public override bool IsUsedUp => IsCrafted;

    internal abstract CardModel RewardCard { get; }

    internal abstract IReadOnlyList<RecipeIngredient> Ingredients { get; }

    internal virtual bool CanBeBorrowedByWeiLaiShen => true;

    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
    [
        HoverTipFactory.FromCard(RewardCard, false)
    ];

    [SavedProperty]
    public bool IsCrafted
    {
        get => _isCrafted;
        set
        {
            AssertMutable();
            _isCrafted = value;
            Status = value ? RelicStatus.Disabled : RelicStatus.Normal;
        }
    }

    public override bool TryModifyRestSiteOptions(
        Player player,
        ICollection<RestSiteOption> options)
    {
        if (!ShaZhaoRecipeSystem.GetAvailableRecipes(player).Any()
            || options.OfType<AssembleShaZhaoRestSiteOption>().Any())
        {
            return false;
        }

        options.Add(new AssembleShaZhaoRestSiteOption(player));
        return true;
    }

    public bool CanHandleRightClickLocal(ModRightClickContext context) =>
        context.Player == Owner
        && !IsCrafted
        && CanBeBorrowedByWeiLaiShen
        && CombatManager.Instance.IsInProgress
        && Owner.GetRelic<WeiLaiShenRelic>() is not null;

    public async Task OnRightClick(ModRightClickExecutionContext context)
    {
        if (!CanHandleRightClickLocal(new ModRightClickContext(
                context.Player,
                context.Model,
                context.Trigger)))
        {
            return;
        }

        var futureBody = Owner.GetRelic<WeiLaiShenRelic>();
        var combatState = Owner.Creature.CombatState;
        if (futureBody is null
            || combatState is null
            || !futureBody.TryUseRecipeBorrow())
        {
            return;
        }

        Flash();
        var card = combatState.CreateCard(RewardCard, Owner);
        await CardPileCmd.AddGeneratedCardToCombat(
            card,
            PileType.Hand,
            Owner,
            CardPilePosition.Bottom);
    }
}
