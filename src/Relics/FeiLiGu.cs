using System.Collections.Generic;
using System.Threading.Tasks;
using GuZhenRen.CardPools;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class FeiLiGu : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/relics/FeiLiGu.png",
        IconOutlinePath: "res://GuZhenRen/images/relics/outline/FeiLiGu.png",
        BigIconPath: "res://GuZhenRen/images/relics/FeiLiGu.png");

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext,
        CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Enemy)
        {
            return;
        }

        foreach (var enemy in participants)
        {
            var strength = enemy.GetPower<StrengthPower>();
            if (!enemy.IsAlive || strength is null || strength.Amount <= 0)
            {
                continue;
            }

            Flash();
            await PowerCmd.ModifyAmount(
                choiceContext,
                strength,
                -1,
                Owner.Creature,
                null);
        }
    }
}
