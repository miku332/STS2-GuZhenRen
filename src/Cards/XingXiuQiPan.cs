using GuZhenRen.CardPools;
using GuZhenRen.Powers;
using GuZhenRen.Tags;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class XingXiuQiPan : AbstractGuWuCard
{
    private static readonly HashSet<ulong> TengNuoUsedByPlayer = [];

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/XingXiuQiPan.png");

    public override IEnumerable<CardTag> Tags =>
        [GuZhenRenTags.ZhiDao, GuZhenRenTags.XianGuWu];

    public XingXiuQiPan()
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
            CombatState.CreateCard<OptionFangHuXingXiuQiPan>(Owner),
            CombatState.CreateCard<OptionZhenChaXingXiuQiPan>(Owner),
            CombatState.CreateCard<OptionTuiSuanXingXiuQiPan>(Owner)
        };
        if (!TengNuoUsedByPlayer.Contains(Owner.NetId))
        {
            choices.Add(CombatState.CreateCard<OptionTengNuoXingXiuQiPan>(Owner));
        }

        var selected = (await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            choices,
            Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 1))).FirstOrDefault();

        switch (selected)
        {
            case OptionFangHuXingXiuQiPan:
                await PowerCmd.Apply<XingLuoQiBuPower>(
                    choiceContext,
                    Owner.Creature,
                    3,
                    Owner.Creature,
                    this);
                break;

            case OptionZhenChaXingXiuQiPan:
                foreach (var enemy in CombatState.HittableEnemies.Where(enemy => enemy.IsAlive))
                {
                    await PowerCmd.Apply<BuMieXingBiaoPower>(
                        choiceContext,
                        enemy,
                        1,
                        Owner.Creature,
                        this);
                }
                break;

            case OptionTuiSuanXingXiuQiPan:
                await PowerCmd.Apply<NianPower>(
                    choiceContext,
                    Owner.Creature,
                    8,
                    Owner.Creature,
                    this);
                break;

            case OptionTengNuoXingXiuQiPan:
                if (TengNuoUsedByPlayer.Add(Owner.NetId))
                {
                    await PowerCmd.Apply<TengNuoExtraTurnPower>(
                        choiceContext,
                        Owner.Creature,
                        1,
                        Owner.Creature,
                        this);
                    PlayerCmd.EndTurn(Owner, false);
                }
                break;
        }
    }

    public static void ResetCombatState()
    {
        TengNuoUsedByPlayer.Clear();
    }
}
