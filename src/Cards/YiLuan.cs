using GuZhenRen.CardPools;
using GuZhenRen.Keywords;
using GuZhenRen.Systems;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Cards;

[RegisterCard(typeof(GuZhenRenCardPool))]
public sealed class YiLuan : GuZhenRenCardTemplate, IProbabilityCard
{
    private const decimal InitialFailureChance = 50m;
    private const int HpLoss = 6;

    public override int Rank => 0;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: "res://GuZhenRen/images/cards/YiLuan.png");

    public override CardPoolModel VisualCardPool => ModelDb.CardPool<StatusCardPool>();

    public override IEnumerable<CardKeyword> CanonicalKeywords =>
    [
        CardKeyword.Unplayable,
        CardKeyword.Ethereal,
        GuZhenRenKeywords.GaiLv
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("HpLoss", HpLoss),
        new FailureChanceVar("FailureChance", InitialFailureChance)
    ];

    public override bool CanBeGeneratedByModifiers => false;

    public override bool CanBeGeneratedInCombat => false;

    protected override bool IsPlayable => false;

    public YiLuan()
        : base(-2, CardType.Status, CardRarity.Token, TargetType.None, false)
    {
    }

    public static bool TryBlockManualPlay(
        PlayCardAction action,
        out Task blockTask)
    {
        blockTask = Task.CompletedTask;

        var card = action.NetCombatCard.ToCardModelOrNull();
        if (card is null || !IsShaZhao(card))
        {
            return false;
        }

        var yiLuan = PileType.Hand.GetPile(card.Owner).Cards
            .FirstOrDefault(IsYiLuan);
        if (yiLuan is null)
        {
            return false;
        }

        var failed = ProbabilitySystem.Roll(
            yiLuan,
            yiLuan.DynamicVars["FailureChance"].BaseValue);

        if (!failed)
        {
            return false;
        }

        blockTask = ResolveFailure(action, yiLuan, card);
        return true;
    }

    private static bool IsYiLuan(CardModel card) =>
        card is YiLuan
        || card.Id.Entry == "GU_ZHEN_REN_CARD_YI_LUAN";

    private static bool IsShaZhao(CardModel card) =>
        card is AbstractShaZhaoCard
        || ShaZhaoIds.Contains(card.Id.Entry);

    private static readonly HashSet<string> ShaZhaoIds =
    [
        "GU_ZHEN_REN_CARD_ANGRY_BIRD",
        "GU_ZHEN_REN_CARD_AN_QI_SHA",
        "GU_ZHEN_REN_CARD_AN_TU_ZHONG_SHAN_BAO",
        "GU_ZHEN_REN_CARD_BAI_GU_ZHAN_CHE",
        "GU_ZHEN_REN_CARD_CHI_XIN",
        "GU_ZHEN_REN_CARD_GUANG_YIN_FEI_REN",
        "GU_ZHEN_REN_CARD_JIAN_HEN_SUO_MING",
        "GU_ZHEN_REN_CARD_JIAN_LANG_SAN_DIE",
        "GU_ZHEN_REN_CARD_JIAN_MIAN_CENG_XIANG_SHI",
        "GU_ZHEN_REN_CARD_LAI_YIN_QU_GUO",
        "GU_ZHEN_REN_CARD_LUAN_FANG_HUN_XIANG_WU",
        "GU_ZHEN_REN_CARD_NI_LIU_HU_SHEN_YIN",
        "GU_ZHEN_REN_CARD_NIE_PAN_HUO",
        "GU_ZHEN_REN_CARD_RAN_NIAN_FEI_SHI",
        "GU_ZHEN_REN_CARD_SAN_SHI_SAN_TIAN_GUANG",
        "GU_ZHEN_REN_CARD_SHANG_FANG_JIE_WA",
        "GU_ZHEN_REN_CARD_SONG_YOU_FENG",
        "GU_ZHEN_REN_CARD_SONG_YOU_FENG_SONG_BIE",
        "GU_ZHEN_REN_CARD_TIAN_PU_GUANG_HE",
        "GU_ZHEN_REN_CARD_WAN_WO",
        "GU_ZHEN_REN_CARD_WAN_WO_DA_SHOU_YIN",
        "GU_ZHEN_REN_CARD_WAN_WU_DA_TONG_BIAN",
        "GU_ZHEN_REN_CARD_WAN_XING_FEI_YING",
        "GU_ZHEN_REN_CARD_WEI_LAI_SHEN",
        "GU_ZHEN_REN_CARD_WU_JIN_XUAN_GUANG_QI",
        "GU_ZHEN_REN_CARD_WU_ZHI_QUAN_XIN_JIAN",
        "GU_ZHEN_REN_CARD_XUE_JIAN_LENG",
        "GU_ZHEN_REN_CARD_XUE_PIAO_LIU",
        "GU_ZHEN_REN_CARD_XUE_RAN_ZHENG_PAO",
        "GU_ZHEN_REN_CARD_YANG_MANG_BEI_HUO_YI",
        "GU_ZHEN_REN_CARD_YIN_GUO_SHEN_SHU",
        "GU_ZHEN_REN_CARD_ZHUI_MING_HUO",
        "GU_ZHEN_REN_CARD_ZHU_MO_BANG"
    ];

    protected override Task OnPlay(
        PlayerChoiceContext choiceContext,
        CardPlay cardPlay) =>
        Task.CompletedTask;

    public void IncreaseBaseChance(decimal percentagePoints)
    {
        var chance = DynamicVars["FailureChance"];
        chance.BaseValue = Math.Clamp(
            chance.BaseValue + percentagePoints,
            0m,
            100m);
    }

    private static async Task ResolveFailure(
        PlayCardAction action,
        CardModel yiLuan,
        CardModel attemptedCard)
    {
        NCardPlayQueue.Instance?.RemoveCardFromQueueForCancellation(action);
        if (LocalContext.IsMe(yiLuan.Owner))
        {
            NPlayerHand.Instance?.TryCancelCardPlay(attemptedCard);
        }

        if (yiLuan.Owner?.PlayerCombatState is null)
        {
            return;
        }

        var choiceContext = new ThrowingPlayerChoiceContext();

        if (attemptedCard.EnergyCost.CostsX)
        {
            await PlayerCmd.LoseEnergy(
                yiLuan.Owner.PlayerCombatState.Energy,
                yiLuan.Owner);
        }

        await CreatureCmd.Damage(
            choiceContext,
            yiLuan.Owner.Creature,
            yiLuan.DynamicVars["HpLoss"].BaseValue,
            ValueProp.Unblockable | ValueProp.Unpowered,
            yiLuan.Owner.Creature,
            yiLuan,
            null);
    }
}

internal sealed class FailureChanceVar : DynamicVar
{
    public FailureChanceVar(string name, decimal baseValue)
        : base(name, baseValue)
    {
    }

    public override void UpdateCardPreview(
        CardModel card,
        CardPreviewMode previewMode,
        Creature? target,
        bool runGlobalHooks)
    {
        PreviewValue = ProbabilitySystem.GetEffectiveChance(
            card,
            BaseValue);
    }
}
