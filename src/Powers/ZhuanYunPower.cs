using GuZhenRen.Systems;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class ZhuanYunPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override bool AllowNegative => false;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/ZhuanYunPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/ZhuanYunPower_p.png");

    public void OnProbabilityRollFailed(CardModel card)
    {
        if (Amount <= 0 || card is not IProbabilityCard probabilityCard)
        {
            return;
        }

        Flash();
        probabilityCard.IncreaseBaseChance(Amount);
    }
}
