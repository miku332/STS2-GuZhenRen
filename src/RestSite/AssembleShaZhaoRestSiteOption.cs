using GuZhenRen.Systems;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.RestSite;

public sealed class AssembleShaZhaoRestSiteOption
    : ModRestSiteOptionTemplate
{
    private readonly Player _owner;

    public AssembleShaZhaoRestSiteOption(Player owner)
        : base(owner)
    {
        _owner = owner;
    }

    public override string OptionId => "GU_ZHEN_REN_ASSEMBLE_SHA_ZHAO";

    public override bool IsEnabled =>
        ShaZhaoRecipeSystem.GetCraftableRecipes(_owner).Any();

    public override LocString? CustomTitle =>
        new("rest_site_ui", "GU_ZHEN_REN_ASSEMBLE_SHA_ZHAO.name");

    public override LocString Description => new(
        "rest_site_ui",
        IsEnabled
            ? "GU_ZHEN_REN_ASSEMBLE_SHA_ZHAO.description"
            : "GU_ZHEN_REN_ASSEMBLE_SHA_ZHAO.descriptionDisabled");

    public override RestSiteOptionAssetProfile AssetProfile => new(
        "res://GuZhenRen/images/ui/campfire_shazhao.png");

    public override async Task<bool> OnSelect()
    {
        await ShaZhaoRecipeSystem.TryCraft(_owner);

        // Returning false keeps the rest site open. Rebuild the buttons after
        // this selection finishes so the recipe option reflects its new state.
        NRestSiteRoom.Instance?.CallDeferred(
            NRestSiteRoom.MethodName.UpdateRestSiteOptions);
        return false;
    }
}
