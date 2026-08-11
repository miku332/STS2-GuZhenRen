using MegaCrit.Sts2.Core.Localization;
using STS2RitsuLib.Interop.AutoRegistration;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class YunDaoDaoHenPower : AbstractDaoHenPower
{
    private const int ProbabilityBonusPerMark = 3;

    public decimal ProbabilityBonus => Amount * ProbabilityBonusPerMark;

    public override LocString Description
    {
        get
        {
            var description = base.Description;
            description.Add("Bonus", ProbabilityBonus);
            return description;
        }
    }
}
