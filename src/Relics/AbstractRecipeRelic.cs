using GuZhenRen.RestSite;
using GuZhenRen.Systems;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Relics;

public abstract class AbstractRecipeRelic : ModRelicTemplate
{
    private bool _isCrafted;

    public override RelicRarity Rarity => RelicRarity.Event;

    public override bool IsUsedUp => IsCrafted;

    internal abstract CardModel RewardCard { get; }

    internal abstract IReadOnlyList<RecipeIngredient> Ingredients { get; }

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
}
