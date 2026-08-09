using GuZhenRen.CardPools;
using GuZhenRen.Powers;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class ZhuMoBang : AbstractXianGuWuCard
{
    private static readonly HashSet<ulong> HuiFuUsedByPlayer = [];

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/ZhuMoBang.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.XueDao];

    public ZhuMoBang()
        : base(1, CardType.Skill, TargetType.None)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(CombatState);

        var choices = new List<CardModel>
        {
            CombatState.CreateCard<OptionZhenChaZhuMoBang>(Owner),
            CombatState.CreateCard<OptionFangHuZhuMoBang>(Owner),
            CombatState.CreateCard<OptionGongFaZhuMoBang>(Owner)
        };
        if (!HuiFuUsedByPlayer.Contains(Owner.NetId))
        {
            choices.Add(CombatState.CreateCard<OptionHuiFuZhuMoBang>(Owner));
        }

        var selected = (await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            choices,
            Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 1))).FirstOrDefault();

        switch (selected)
        {
            case OptionZhenChaZhuMoBang:
                await UseZhenCha(choiceContext);
                break;

            case OptionFangHuZhuMoBang:
                await CreatureCmd.GainBlock(
                    Owner.Creature,
                    8,
                    ValueProp.Move,
                    cardPlay,
                    false);
                await PowerCmd.Apply<XueDianXingBanPower>(
                    choiceContext,
                    Owner.Creature,
                    1,
                    Owner.Creature,
                    this);
                break;

            case OptionGongFaZhuMoBang:
                await UseGongFa(choiceContext);
                break;

            case OptionHuiFuZhuMoBang:
                if (HuiFuUsedByPlayer.Add(Owner.NetId))
                {
                    await UseHuiFu(choiceContext);
                }
                break;
        }
    }

    private async Task UseZhenCha(PlayerChoiceContext choiceContext)
    {
        if (Owner.Creature.GetPower<XueYuanPower>() is null)
        {
            await PowerCmd.Apply<XueYuanPower>(
                choiceContext,
                Owner.Creature,
                1,
                Owner.Creature,
                this);
        }

        foreach (var enemy in CombatState!.HittableEnemies.Where(enemy => enemy.IsAlive))
        {
            await PowerCmd.Apply<XueYuanMarkPower>(
                choiceContext,
                enemy,
                1,
                Owner.Creature,
                this);
        }
    }

    private async Task UseGongFa(PlayerChoiceContext choiceContext)
    {
        foreach (var enemy in CombatState!.HittableEnemies.Where(enemy => enemy.IsAlive).ToList())
        {
            var hits = (int)Math.Max(0, enemy.GetPower<XueYuanMarkPower>()?.Amount ?? 0);
            for (var i = 0; i < hits && enemy.IsAlive; i++)
            {
                await CreatureCmd.Damage(
                    choiceContext,
                    enemy,
                    18,
                    ValueProp.Unpowered,
                    Owner.Creature,
                    null);
            }
        }
    }

    private async Task UseHuiFu(PlayerChoiceContext choiceContext)
    {
        decimal totalDrained = 0;
        foreach (var enemy in CombatState!.HittableEnemies.Where(enemy => enemy.IsAlive).ToList())
        {
            if ((enemy.GetPower<XueYuanMarkPower>()?.Amount ?? 0) <= 0)
            {
                continue;
            }

            var amountToDrain = Math.Min(5m, enemy.CurrentHp);
            if (amountToDrain <= 0)
            {
                continue;
            }

            var hpBefore = enemy.CurrentHp;
            await CreatureCmd.Damage(
                choiceContext,
                enemy,
                amountToDrain,
                ValueProp.Unblockable | ValueProp.Unpowered,
                Owner.Creature,
                null);
            totalDrained += Math.Max(0, hpBefore - enemy.CurrentHp);
        }

        if (totalDrained > 0)
        {
            await CreatureCmd.Heal(Owner.Creature, totalDrained);
        }
    }

    public static void ResetCombatState()
    {
        HuiFuUsedByPlayer.Clear();
    }
}
