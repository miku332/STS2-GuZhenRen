using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models.Powers;
using GuZhenRen.Monsters;
using GuZhenRen.Patches;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class LongYuShangBinPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override LocString Description
    {
        get
        {
            var description = new LocString(
                "powers",
                "GU_ZHEN_REN_POWER_LONG_YU_SHANG_BIN_POWER.description");
            description.Add("Amount", Amount);
            description.Add("Strength", Math.Floor(Amount / 10m));
            return description;
        }
    }

    public override PowerAssetProfile AssetProfile => new(
        IconPath:
            "res://GuZhenRen/images/powers/LongYuShangBinPower.png",
        BigIconPath:
            "res://GuZhenRen/images/powers/LongYuShangBinPower_p.png");

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Enemy
            || !Owner.IsAlive
            || !participants.Contains(Owner)
            || Amount <= 0)
        {
            return;
        }

        var longGong = Owner.Monster as LongGong;
        if (longGong is not null
            && longGong.ConsumeLongYuSkip())
        {
            return;
        }

        Flash();
        if (Owner.MaxHp <= Amount)
        {
            await CreatureCmd.SetMaxHp(Owner, 1);
            using (QiHuState.EnterBypassScope())
            {
                await CreatureCmd.Kill(
                    Owner,
                    force: longGong?.IsInSecondPhase == true);
            }
            return;
        }

        await CreatureCmd.SetMaxHp(Owner, Owner.MaxHp - Amount);

        var strength = Math.Floor(Amount / 10m);
        if (Owner.IsAlive && strength > 0)
        {
            await PowerCmd.Apply<StrengthPower>(
                choiceContext,
                Owner,
                strength,
                Owner,
                null);
        }
    }
}
