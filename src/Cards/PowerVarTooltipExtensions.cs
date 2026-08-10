using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Cards.DynamicVars;

namespace GuZhenRen.Cards;

internal static class PowerVarTooltipExtensions
{
    public static PowerVar<T> WithPowerTooltip<T>(this PowerVar<T> powerVar)
        where T : PowerModel
    {
        powerVar.WithTooltip(static _ => HoverTipFactory.FromPower<T>());
        return powerVar;
    }
}
