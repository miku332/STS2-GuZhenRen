using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Powers;

[RegisterPower]
public sealed class YangMangBeiHuoYiPower : ModPowerTemplate
{
    private const int BurnAmount = 1;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/powers/YangMangBeiHuoYiPower.png",
        BigIconPath: "res://GuZhenRen/images/powers/YangMangBeiHuoYiPower_p.png");

    public override async Task AfterDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        DamageResult result,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (target != Owner
            || dealer is null
            || dealer == Owner
            || Amount <= 0)
        {
            return;
        }

        Flash();
        await PowerCmd.Apply<FenShaoPower>(
            choiceContext,
            dealer,
            BurnAmount,
            Owner,
            cardSource);
        await PowerCmd.ModifyAmount(
            choiceContext,
            this,
            -1,
            Owner,
            cardSource);
    }

    public override async Task AfterPlayerTurnStart(
        PlayerChoiceContext choiceContext,
        MegaCrit.Sts2.Core.Entities.Players.Player player)
    {
        if (player == Owner.Player)
        {
            await PowerCmd.Remove(this);
        }
    }
}
