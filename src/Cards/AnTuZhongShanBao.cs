using GuZhenRen.CardPools;
using GuZhenRen.Enchantments;
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
public sealed class AnTuZhongShanBao : AbstractXianGuWuCard
{
    private static readonly HashSet<ulong> BuDongRuShanUsedByPlayer = [];

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/AnTuZhongShanBao.png");

    public override IEnumerable<CardTag> Tags => [GuZhenRenTags.TuDao];

    public AnTuZhongShanBao()
        : base(2, CardType.Skill, TargetType.None)
    {
    }

    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(CombatState);

        var choices = new List<CardModel>
        {
            CreateOption<OptionJiTuChengShanAnTuZhongShanBao>(),
            CreateOption<OptionRuTuWeiAnAnTuZhongShanBao>(),
            CreateOption<OptionJuanTuChongLaiAnTuZhongShanBao>()
        };
        if (!BuDongRuShanUsedByPlayer.Contains(Owner.NetId))
        {
            choices.Add(CreateOption<OptionBuDongRuShanAnTuZhongShanBao>());
        }

        var selected = (await CardSelectCmd.FromSimpleGrid(
            choiceContext,
            choices,
            Owner,
            new CardSelectorPrefs(SelectionScreenPrompt, 1))).FirstOrDefault();

        switch (selected)
        {
            case OptionJiTuChengShanAnTuZhongShanBao:
                await UseJiTuChengShan(cardPlay);
                break;

            case OptionRuTuWeiAnAnTuZhongShanBao:
                UseRuTuWeiAn();
                break;

            case OptionJuanTuChongLaiAnTuZhongShanBao:
                await UseJuanTuChongLai(choiceContext);
                break;

            case OptionBuDongRuShanAnTuZhongShanBao:
                await UseBuDongRuShan(choiceContext, cardPlay);
                break;
        }
    }

    public static void ResetCombatState()
    {
        BuDongRuShanUsedByPlayer.Clear();
    }

    private TOption CreateOption<TOption>() where TOption : CardModel
    {
        var option = CombatState!.CreateCard<TOption>(Owner);
        if (option is OptionJiTuChengShanAnTuZhongShanBao jiTuChengShan)
        {
            jiTuChengShan.SetTriggerCount(CountTuDaoCards(PileType.Hand));
        }

        if (option is OptionJuanTuChongLaiAnTuZhongShanBao juanTuChongLai)
        {
            juanTuChongLai.SetCardCount(CountTuDaoCards(PileType.Discard));
        }

        return option;
    }

    private int CountTuDaoCards(PileType pileType) =>
        pileType
            .GetPile(Owner)
            .Cards
            .Count(card => GuZhenRenTagRules.HasEffectiveTag(
                card,
                GuZhenRenTags.TuDao));

    private async Task UseJiTuChengShan(CardPlay cardPlay)
    {
        var count = CountTuDaoCards(PileType.Hand);
        for (var i = 0; i < count; i++)
        {
            await CreatureCmd.GainBlock(
                Owner.Creature,
                8,
                MegaCrit.Sts2.Core.ValueProps.ValueProp.Move,
                cardPlay,
                false);
        }
    }

    private void UseRuTuWeiAn()
    {
        var enchantment = ModelDb.Enchantment<HuaShiEnchantment>();
        foreach (var card in PileType.Hand.GetPile(Owner).Cards.ToList())
        {
            if (enchantment.CanEnchant(card))
            {
                CardCmd.Enchant<HuaShiEnchantment>(card, 3);
            }
        }
    }

    private async Task UseJuanTuChongLai(
        PlayerChoiceContext choiceContext)
    {
        var cards = PileType.Discard
            .GetPile(Owner)
            .Cards
            .Where(card => GuZhenRenTagRules.HasEffectiveTag(
                card,
                GuZhenRenTags.TuDao))
            .ToList();
        if (cards.Count == 0)
        {
            return;
        }

        await CardPileCmd.Add(
            cards,
            PileType.Draw,
            CardPilePosition.Random);
        await PowerCmd.Apply<JiTuPower>(
            choiceContext,
            Owner.Creature,
            cards.Count * 3,
            Owner.Creature,
            this);
    }

    private async Task UseBuDongRuShan(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay)
    {
        if (!BuDongRuShanUsedByPlayer.Add(Owner.NetId))
        {
            return;
        }

        await CreatureCmd.GainBlock(
            Owner.Creature,
            30,
            MegaCrit.Sts2.Core.ValueProps.ValueProp.Move,
            cardPlay,
            false);
        await PowerCmd.Apply<JiTuPower>(
            choiceContext,
            Owner.Creature,
            30,
            Owner.Creature,
            this);
    }
}
