using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Cards.DynamicVars;

namespace GuZhenRen.Cards;

internal static class PowerVarTooltipExtensions
{
    public static PowerVar<T> WithPowerTooltip<T>(this PowerVar<T> powerVar)
        where T : PowerModel
    {
        powerVar.WithTooltip(static _ => CreatePowerTooltip<T>());
        return powerVar;
    }

    private static IHoverTip CreatePowerTooltip<T>()
        where T : PowerModel
    {
        var power = ModelDb.Power<T>();

        if (typeof(T) == typeof(ConstrictPower))
        {
            var constrictDescription = new LocString(
                "powers",
                "GU_ZHEN_REN_POWER_CONSTRICT_PREVIEW.description");
            return new HoverTip(
                power,
                constrictDescription.GetFormattedText(),
                isSmart: false);
        }

        var description = power.Description;
        description.Add("Amount", "[blue]X[/blue]");
        description.Add("singleStarIcon", "[img]res://images/packed/sprite_fonts/star_icon.png[/img]");
        description.Add("energyPrefix", EnergyIconHelper.GetPrefix(power));
        return new HoverTip(power, description.GetFormattedText(), isSmart: false);
    }
}
