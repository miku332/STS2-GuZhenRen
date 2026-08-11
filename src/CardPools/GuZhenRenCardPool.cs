using Godot;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Utils;

namespace GuZhenRen.CardPools;

[RegisterSharedCardPool]
public sealed class GuZhenRenCardPool : TypeListCardPoolModel
{
    public override string Title => "guzhenren";

    public override string EnergyColorName => "guzhenren";

    public override string? TextEnergyIconPath =>
        "res://GuZhenRen/images/energy_guzhenren.png";

    public override string? BigEnergyIconPath =>
        "res://GuZhenRen/images/energy_guzhenren_big.png";

    public override Color DeckEntryCardColor => new(0.62f, 0.63f, 0.67f);

    public override Color EnergyOutlineColor => new(0.62f, 0.63f, 0.67f);

    private static readonly Material? _poolFrameMaterial =
        MaterialUtils.CreateReplaceHueShaderMaterial(0.62f, 0.63f, 0.67f);

    public override Material? PoolFrameMaterial => _poolFrameMaterial;

    public override bool IsColorless => false;
}
