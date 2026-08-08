using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Interop.AutoRegistration;

namespace GuZhenRen.CardPools;

[RegisterSharedPotionPool]
public sealed class GuZhenRenPotionPool : TypeListPotionPoolModel
{
    public override string EnergyColorName => "guzhenren";

    public override string? TextEnergyIconPath =>
        "res://GuZhenRen/images/energy_guzhenren.png";

    public override string? BigEnergyIconPath =>
        "res://GuZhenRen/images/energy_guzhenren_big.svg";
}
