using GuZhenRen.CardPools;
using GuZhenRen.Powers;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class BaiGuZhanChe : AbstractGuWuCard
{
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/BaiGuZhanChe.png");

    public override IEnumerable<CardTag> Tags =>
        [GuZhenRenTags.GuDao, GuZhenRenTags.FanGuWu];

    public BaiGuZhanChe()
        : base(1, CardType.Skill, TargetType.None)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(CombatState);

        var gongFa = CombatState.CreateCard<OptionGongFaBaiGuZhanChe>(Owner);
        gongFa.SetCurrentBlock(Owner.Creature.Block);
        var choices = new List<CardModel>
        {
            gongFa,
            CombatState.CreateCard<OptionFangHuBaiGuZhanChe>(Owner),
            CombatState.CreateCard<OptionYiDongBaiGuZhanChe>(Owner)
        };
        var selected = (await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            choices,
            Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 1))).FirstOrDefault();

        switch (selected)
        {
            case OptionGongFaBaiGuZhanChe:
                await UseGongFa(choiceContext);
                break;

            case OptionFangHuBaiGuZhanChe:
                await UseFangHu(choiceContext);
                break;

            case OptionYiDongBaiGuZhanChe:
                await UseYiDong(choiceContext, cardPlay);
                break;
        }
    }

    private async Task UseGongFa(PlayerChoiceContext choiceContext)
    {
        var enemies = CombatState!.HittableEnemies
            .Where(enemy => enemy.IsAlive)
            .ToList();
        var damage = Owner.Creature.Block;
        if (damage > 0)
        {
            foreach (var enemy in enemies)
            {
                await CreatureCmd.Damage(
                    choiceContext,
                    enemy,
                    damage,
                    ValueProp.Unpowered,
                    Owner.Creature,
                    null,
                    null);
            }
        }

        foreach (var enemy in enemies.Where(enemy => enemy.IsAlive))
        {
            await PowerCmd.Apply<WeakPower>(
                choiceContext,
                enemy,
                2,
                Owner.Creature,
                this);
        }
    }

    private async Task UseFangHu(PlayerChoiceContext choiceContext)
    {
        await PowerCmd.Apply<ThornsPower>(
            choiceContext,
            Owner.Creature,
            3,
            Owner.Creature,
            this);
        await PowerCmd.Apply<GuCiPower>(
            choiceContext,
            Owner.Creature,
            3,
            Owner.Creature,
            this);
    }

    private async Task UseYiDong(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(
            Owner.Creature,
            18,
            ValueProp.Move,
            cardPlay,
            false);

        var enemies = CombatState!.HittableEnemies
            .Where(enemy => enemy.IsAlive)
            .ToList();
        foreach (var enemy in enemies)
        {
            var hits = enemy.GetPowerAmount<WeakPower>();
            for (var i = 0; i < hits && Owner.Creature.IsAlive; i++)
            {
                await CreatureCmd.Damage(
                    choiceContext,
                    Owner.Creature,
                    1,
                    ValueProp.Move,
                    enemy,
                    null,
                    null);
            }
        }
    }
}
