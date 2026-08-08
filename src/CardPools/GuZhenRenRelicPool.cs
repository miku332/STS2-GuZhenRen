using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.CardPools;

[RegisterSharedRelicPool]
public sealed class GuZhenRenRelicPool : TypeListRelicPoolModel
{
    public override string EnergyColorName => "guzhenren";

    public override string? TextEnergyIconPath =>
        "res://GuZhenRen/images/energy_guzhenren.png";

    public override string? BigEnergyIconPath =>
        "res://GuZhenRen/images/energy_guzhenren_big.svg";
}
