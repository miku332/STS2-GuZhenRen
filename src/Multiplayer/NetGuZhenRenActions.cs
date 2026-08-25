using System.Text.Json;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using GuZhenRen.Cards;
using GuZhenRen.Potions;
using GuZhenRen.Powers;
using GuZhenRen.Patches;
using GuZhenRen.Relics;
using GuZhenRen.Systems;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Networking.ManagedActions;

namespace GuZhenRen.Multiplayer;

internal readonly record struct PaiNanDrawPayload(
    ulong TargetNetId,
    uint TriggerCardIndex,
    int DrawAmount,
    uint[] CardIndices);

internal readonly record struct DiMaiBlockPayload(
    ulong TargetNetId,
    int Block);

internal readonly record struct TongXinAutoPlayPayload(
    ulong OwnerNetId,
    uint CardIndex,
    uint? TargetCombatId);

internal readonly record struct LiQiTriggerPayload(
    uint CardIndex,
    uint? TargetCombatId);

internal readonly record struct LiQiAutoPlayPayload(
    ulong OwnerNetId,
    LiQiTriggerPayload[] Triggers);

internal readonly record struct RanNianFeiShiTriggerPayload(
    uint CardIndex,
    uint? TargetCombatId);

internal readonly record struct RanNianFeiShiAutoPlayPayload(
    ulong OwnerNetId,
    RanNianFeiShiTriggerPayload[] Triggers);

internal readonly record struct YiXinErYongAutoPlayPayload(
    ulong OwnerNetId,
    uint CardIndex,
    uint? TargetCombatId,
    int CapturedXValue);

internal readonly record struct BloodcrazeCardPlayPayload(
    uint CardIndex,
    uint? TargetCombatId,
    int CapturedXValue);

internal readonly record struct BloodcrazeAutoPlayPayload(
    ulong OwnerNetId,
    BloodcrazeCardPlayPayload[] Cards);

internal enum AiQingPositiveOutcome : byte
{
    Card = 1,
    Relic = 2,
    Heal = 3,
    Escape = 4
}

internal enum AiQingNegativeOutcome : byte
{
    Damage = 1,
    LoseMaxHp = 2,
    Strength = 3,
    Dexterity = 4
}

internal readonly record struct AiQingGuPayload(
    ulong OwnerNetId,
    uint CardIndex,
    AiQingPositiveOutcome PositiveOutcome,
    string? PositiveModelId,
    RelicRarity PositiveRelicRarity,
    AiQingNegativeOutcome NegativeOutcome);

internal readonly record struct BenMingGuSelectionPayload(string CardModelId);

internal readonly record struct FuRenXinSlotPayload(
    ulong OwnerNetId,
    int Slot,
    int Poison);

internal readonly record struct FuRenXinGrowthPayload(
    FuRenXinSlotPayload[] Potions);

internal readonly record struct WeiLaiShenReturnPayload(
    ulong OwnerNetId,
    bool AddCombatCopy);

internal readonly record struct JianMianCengXiangShiReturnPayload(
    ulong OwnerNetId,
    bool AddCombatCopy);

internal readonly record struct TouShengUsePayload(ulong OwnerNetId);

internal readonly record struct WeiLaiShenRecipeBorrowPayload(
    ulong OwnerNetId,
    string RecipeRelicModelId);

internal readonly record struct UniquenessCleanupPayload;

internal readonly record struct ShaZhaoRecipeRewardsPayload;

internal readonly record struct YingShengChongTargetPayload(
    uint? PowerOwnerCombatId,
    ulong AffectedPlayerNetId,
    uint? CardIndex);

internal readonly record struct YingShengChongKillPayload(
    ulong AffectedPlayerNetId);

internal static class NetGuZhenRenActions
{
    private const int MaxRetryAttempts = 100;
    private const double RetryDelaySeconds = 0.05;

    private static readonly RitsuLibManagedNetActionDescriptor<PaiNanDrawPayload>
        PaiNanDrawDescriptor = new(
            ModuleId: Entry.ModId,
            ActionKey: "pai_nan_draw_v2",
            Serialize: static payload => JsonSerializer.SerializeToUtf8Bytes(payload),
            Deserialize: static bytes => JsonSerializer.Deserialize<PaiNanDrawPayload>(bytes),
            Execute: static context => PaiNanPower.ExecuteManagedDrawAsync(context),
            ActionType: GameActionType.CombatPlayPhaseOnly);

    private static readonly RitsuLibManagedNetActionDescriptor<DiMaiBlockPayload>
        DiMaiBlockDescriptor = new(
            ModuleId: Entry.ModId,
            ActionKey: "di_mai_block_v2",
            Serialize: static payload => JsonSerializer.SerializeToUtf8Bytes(payload),
            Deserialize: static bytes => JsonSerializer.Deserialize<DiMaiBlockPayload>(bytes),
            Execute: static context => DiMai.ExecuteManagedBlockAsync(context),
            ActionType: GameActionType.CombatPlayPhaseOnly);

    private static readonly RitsuLibManagedNetActionDescriptor<TongXinAutoPlayPayload>
        TongXinAutoPlayDescriptor = new(
            ModuleId: Entry.ModId,
            ActionKey: "tong_xin_auto_play_v2",
            Serialize: static payload => JsonSerializer.SerializeToUtf8Bytes(payload),
            Deserialize: static bytes => JsonSerializer.Deserialize<TongXinAutoPlayPayload>(bytes),
            Execute: static context => TongXinPower.ExecuteManagedAutoPlayAsync(context),
            ActionType: GameActionType.CombatPlayPhaseOnly);

    private static readonly RitsuLibManagedNetActionDescriptor<LiQiAutoPlayPayload>
        LiQiAutoPlayDescriptor = new(
            ModuleId: Entry.ModId,
            ActionKey: "li_qi_auto_play_v1",
            Serialize: static payload => JsonSerializer.SerializeToUtf8Bytes(payload),
            Deserialize: static bytes => JsonSerializer.Deserialize<LiQiAutoPlayPayload>(bytes),
            Execute: static context => LiQiPower.ExecuteManagedAutoPlayAsync(context),
            ActionType: GameActionType.CombatPlayPhaseOnly);

    private static readonly RitsuLibManagedNetActionDescriptor<RanNianFeiShiAutoPlayPayload>
        RanNianFeiShiAutoPlayDescriptor = new(
            ModuleId: Entry.ModId,
            ActionKey: "ran_nian_fei_shi_auto_play_v1",
            Serialize: static payload => JsonSerializer.SerializeToUtf8Bytes(payload),
            Deserialize: static bytes => JsonSerializer.Deserialize<RanNianFeiShiAutoPlayPayload>(bytes),
            Execute: static context => RanNianFeiShi.ExecuteManagedAutoPlayAsync(context),
            ActionType: GameActionType.CombatPlayPhaseOnly);

    private static readonly RitsuLibManagedNetActionDescriptor<YiXinErYongAutoPlayPayload>
        YiXinErYongAutoPlayDescriptor = new(
            ModuleId: Entry.ModId,
            ActionKey: "yi_xin_er_yong_auto_play_v2",
            Serialize: static payload => JsonSerializer.SerializeToUtf8Bytes(payload),
            Deserialize: static bytes => JsonSerializer.Deserialize<YiXinErYongAutoPlayPayload>(bytes),
            Execute: static context => YiXinErYongPower.ExecuteManagedAutoPlayAsync(context),
            ActionType: GameActionType.CombatPlayPhaseOnly);

    private static readonly RitsuLibManagedNetActionDescriptor<BloodcrazeAutoPlayPayload>
        BloodcrazeAutoPlayDescriptor = new(
            ModuleId: Entry.ModId,
            ActionKey: "bloodcraze_auto_play_v2",
            Serialize: static payload => JsonSerializer.SerializeToUtf8Bytes(payload),
            Deserialize: static bytes => JsonSerializer.Deserialize<BloodcrazeAutoPlayPayload>(bytes),
            Execute: static context => XueKuangGu.ExecuteManagedAutoPlayAsync(context),
            ActionType: GameActionType.CombatPlayPhaseOnly);

    private static readonly RitsuLibManagedNetActionDescriptor<AiQingGuPayload>
        AiQingGuDescriptor = new(
            ModuleId: Entry.ModId,
            ActionKey: "ai_qing_gu_resolve_v2",
            Serialize: static payload => JsonSerializer.SerializeToUtf8Bytes(payload),
            Deserialize: static bytes => JsonSerializer.Deserialize<AiQingGuPayload>(bytes),
            Execute: static context => AiQingGu.ExecuteManagedDrawAsync(context),
            ActionType: GameActionType.CombatPlayPhaseOnly);

    private static readonly RitsuLibManagedNetActionDescriptor<BenMingGuSelectionPayload>
        BenMingGuSelectionDescriptor = new(
            ModuleId: Entry.ModId,
            ActionKey: "ben_ming_gu_selection_v1",
            Serialize: static payload => JsonSerializer.SerializeToUtf8Bytes(payload),
            Deserialize: static bytes => JsonSerializer.Deserialize<BenMingGuSelectionPayload>(bytes),
            Execute: static context => BenMingGuSelectionCoordinator.ExecuteManagedSelectionAsync(context),
            ActionType: GameActionType.NonCombat);

    private static readonly RitsuLibManagedNetActionDescriptor<FuRenXinGrowthPayload>
        FuRenXinGrowthDescriptor = new(
            ModuleId: Entry.ModId,
            ActionKey: "fu_ren_xin_growth_v2",
            Serialize: static payload => JsonSerializer.SerializeToUtf8Bytes(payload),
            Deserialize: static bytes => JsonSerializer.Deserialize<FuRenXinGrowthPayload>(bytes),
            Execute: static context => FuRenXin.ExecuteManagedGrowthAsync(context),
            ActionType: GameActionType.Any);

    private static readonly RitsuLibManagedNetActionDescriptor<WeiLaiShenReturnPayload>
        WeiLaiShenReturnDescriptor = new(
            ModuleId: Entry.ModId,
            ActionKey: "wei_lai_shen_return_v1",
            Serialize: static payload => JsonSerializer.SerializeToUtf8Bytes(payload),
            Deserialize: static bytes => JsonSerializer.Deserialize<WeiLaiShenReturnPayload>(bytes),
            Execute: static context => WeiLaiShenRelic.ExecuteManagedReturnAsync(context),
            ActionType: GameActionType.Any);

    private static readonly RitsuLibManagedNetActionDescriptor<JianMianCengXiangShiReturnPayload>
        JianMianCengXiangShiReturnDescriptor = new(
            ModuleId: Entry.ModId,
            ActionKey: "jian_mian_ceng_xiang_shi_return_v1",
            Serialize: static payload => JsonSerializer.SerializeToUtf8Bytes(payload),
            Deserialize: static bytes => JsonSerializer.Deserialize<JianMianCengXiangShiReturnPayload>(bytes),
            Execute: static context => JianMianCengXiangShiRelic.ExecuteManagedReturnAsync(context),
            ActionType: GameActionType.Any);

    private static readonly RitsuLibManagedNetActionDescriptor<TouShengUsePayload>
        TouShengUseDescriptor = new(
            ModuleId: Entry.ModId,
            ActionKey: "tou_sheng_use_v1",
            Serialize: static payload => JsonSerializer.SerializeToUtf8Bytes(payload),
            Deserialize: static bytes => JsonSerializer.Deserialize<TouShengUsePayload>(bytes),
            Execute: static context => TouSheng.ExecuteManagedUseAsync(context),
            ActionType: GameActionType.CombatPlayPhaseOnly);

    private static readonly RitsuLibManagedNetActionDescriptor<WeiLaiShenRecipeBorrowPayload>
        WeiLaiShenRecipeBorrowDescriptor = new(
            ModuleId: Entry.ModId,
            ActionKey: "wei_lai_shen_recipe_borrow_v1",
            Serialize: static payload => JsonSerializer.SerializeToUtf8Bytes(payload),
            Deserialize: static bytes => JsonSerializer.Deserialize<WeiLaiShenRecipeBorrowPayload>(bytes),
            Execute: static context => AbstractRecipeRelic.ExecuteManagedBorrowAsync(context),
            ActionType: GameActionType.CombatPlayPhaseOnly);

    private static readonly RitsuLibManagedNetActionDescriptor<UniquenessCleanupPayload>
        UniquenessCleanupDescriptor = new(
            ModuleId: Entry.ModId,
            ActionKey: "xian_gu_uniqueness_cleanup_v1",
            Serialize: static payload => JsonSerializer.SerializeToUtf8Bytes(payload),
            Deserialize: static bytes => JsonSerializer.Deserialize<UniquenessCleanupPayload>(bytes),
            Execute: static context => XianGuUpgradeUniquenessPatch.ExecuteManagedCleanupAsync(context),
            ActionType: GameActionType.Any);

    private static readonly RitsuLibManagedNetActionDescriptor<ShaZhaoRecipeRewardsPayload>
        ShaZhaoRecipeRewardsDescriptor = new(
            ModuleId: Entry.ModId,
            ActionKey: "sha_zhao_recipe_rewards_v1",
            Serialize: static payload => JsonSerializer.SerializeToUtf8Bytes(payload),
            Deserialize: static bytes => JsonSerializer.Deserialize<ShaZhaoRecipeRewardsPayload>(bytes),
            Execute: static context => ShaZhaoRecipeDropSystem.ExecuteManagedRewardsAsync(context),
            ActionType: GameActionType.NonCombat);

    private static readonly RitsuLibManagedNetActionDescriptor<YingShengChongTargetPayload>
        YingShengChongTargetDescriptor = new(
            ModuleId: Entry.ModId,
            ActionKey: "ying_sheng_chong_target_v1",
            Serialize: static payload => JsonSerializer.SerializeToUtf8Bytes(payload),
            Deserialize: static bytes => JsonSerializer.Deserialize<YingShengChongTargetPayload>(bytes),
            Execute: static context => YingShengChongPower.ExecuteManagedTargetAsync(context),
            ActionType: GameActionType.CombatPlayPhaseOnly);

    private static readonly RitsuLibManagedNetActionDescriptor<YingShengChongKillPayload>
        YingShengChongKillDescriptor = new(
            ModuleId: Entry.ModId,
            ActionKey: "ying_sheng_chong_kill_v1",
            Serialize: static payload => JsonSerializer.SerializeToUtf8Bytes(payload),
            Deserialize: static bytes => JsonSerializer.Deserialize<YingShengChongKillPayload>(bytes),
            Execute: static context => YingShengChongPower.ExecuteManagedKillAsync(context),
            ActionType: GameActionType.Any);

    internal static void Register()
    {
        RitsuLibManagedNetActions.Register(PaiNanDrawDescriptor);
        RitsuLibManagedNetActions.Register(DiMaiBlockDescriptor);
        RitsuLibManagedNetActions.Register(TongXinAutoPlayDescriptor);
        RitsuLibManagedNetActions.Register(LiQiAutoPlayDescriptor);
        RitsuLibManagedNetActions.Register(RanNianFeiShiAutoPlayDescriptor);
        RitsuLibManagedNetActions.Register(YiXinErYongAutoPlayDescriptor);
        RitsuLibManagedNetActions.Register(BloodcrazeAutoPlayDescriptor);
        RitsuLibManagedNetActions.Register(AiQingGuDescriptor);
        RitsuLibManagedNetActions.Register(BenMingGuSelectionDescriptor);
        RitsuLibManagedNetActions.Register(FuRenXinGrowthDescriptor);
        RitsuLibManagedNetActions.Register(WeiLaiShenReturnDescriptor);
        RitsuLibManagedNetActions.Register(JianMianCengXiangShiReturnDescriptor);
        RitsuLibManagedNetActions.Register(TouShengUseDescriptor);
        RitsuLibManagedNetActions.Register(WeiLaiShenRecipeBorrowDescriptor);
        RitsuLibManagedNetActions.Register(UniquenessCleanupDescriptor);
        RitsuLibManagedNetActions.Register(ShaZhaoRecipeRewardsDescriptor);
        RitsuLibManagedNetActions.Register(YingShengChongTargetDescriptor);
        RitsuLibManagedNetActions.Register(YingShengChongKillDescriptor);
    }

    internal static bool RequestPaiNan(PaiNanDrawPayload payload) =>
        RitsuLibManagedNetActions.Request(
            RunManager.Instance,
            PaiNanDrawDescriptor,
            payload);

    internal static bool RequestDiMai(DiMaiBlockPayload payload) =>
        RitsuLibManagedNetActions.Request(
            RunManager.Instance,
            DiMaiBlockDescriptor,
            payload);

    internal static bool RequestTongXin(TongXinAutoPlayPayload payload) =>
        RitsuLibManagedNetActions.Request(
            RunManager.Instance,
            TongXinAutoPlayDescriptor,
            payload);

    internal static bool RequestLiQi(LiQiAutoPlayPayload payload) =>
        RitsuLibManagedNetActions.Request(
            RunManager.Instance,
            LiQiAutoPlayDescriptor,
            payload);

    internal static bool RequestRanNianFeiShi(
        RanNianFeiShiAutoPlayPayload payload) =>
        RitsuLibManagedNetActions.Request(
            RunManager.Instance,
            RanNianFeiShiAutoPlayDescriptor,
            payload);

    internal static bool RequestYiXinErYong(YiXinErYongAutoPlayPayload payload) =>
        RitsuLibManagedNetActions.Request(
            RunManager.Instance,
            YiXinErYongAutoPlayDescriptor,
            payload);

    internal static bool RequestBloodcraze(BloodcrazeAutoPlayPayload payload) =>
        RitsuLibManagedNetActions.Request(
            RunManager.Instance,
            BloodcrazeAutoPlayDescriptor,
            payload);

    internal static bool RequestAiQingGu(AiQingGuPayload payload) =>
        RitsuLibManagedNetActions.Request(
            RunManager.Instance,
            AiQingGuDescriptor,
            payload);

    internal static bool RequestBenMingGuSelection(
        BenMingGuSelectionPayload payload) =>
        RitsuLibManagedNetActions.Request(
            RunManager.Instance,
            BenMingGuSelectionDescriptor,
            payload);

    internal static bool RequestFuRenXinGrowth(FuRenXinGrowthPayload payload) =>
        RitsuLibManagedNetActions.Request(
            RunManager.Instance,
            FuRenXinGrowthDescriptor,
            payload);

    internal static bool RequestWeiLaiShenReturn(
        WeiLaiShenReturnPayload payload) =>
        RitsuLibManagedNetActions.Request(
            RunManager.Instance,
            WeiLaiShenReturnDescriptor,
            payload);

    internal static bool RequestJianMianCengXiangShiReturn(
        JianMianCengXiangShiReturnPayload payload) =>
        RitsuLibManagedNetActions.Request(
            RunManager.Instance,
            JianMianCengXiangShiReturnDescriptor,
            payload);

    internal static bool RequestTouShengUse(TouShengUsePayload payload) =>
        RitsuLibManagedNetActions.Request(
            RunManager.Instance,
            TouShengUseDescriptor,
            payload);

    internal static bool RequestWeiLaiShenRecipeBorrow(
        WeiLaiShenRecipeBorrowPayload payload) =>
        RitsuLibManagedNetActions.Request(
            RunManager.Instance,
            WeiLaiShenRecipeBorrowDescriptor,
            payload);

    internal static bool RequestUniquenessCleanup() =>
        RitsuLibManagedNetActions.Request(
            RunManager.Instance,
            UniquenessCleanupDescriptor,
            default);

    internal static bool RequestShaZhaoRecipeRewards() =>
        RitsuLibManagedNetActions.Request(
            RunManager.Instance,
            ShaZhaoRecipeRewardsDescriptor,
            default);

    internal static bool RequestYingShengChongTarget(
        YingShengChongTargetPayload payload) =>
        RitsuLibManagedNetActions.Request(
            RunManager.Instance,
            YingShengChongTargetDescriptor,
            payload);

    internal static bool RequestYingShengChongKill(
        YingShengChongKillPayload payload) =>
        RitsuLibManagedNetActions.Request(
            RunManager.Instance,
            YingShengChongKillDescriptor,
            payload);

    internal static void RequestWithRetry(
        Func<bool> request,
        Func<bool> canRetry,
        Action onTimeout,
        string actionName,
        bool requiresCombat = true)
    {
        if (request())
        {
            return;
        }

        TaskHelper.RunSafely(RetryRequestAsync(
            request,
            canRetry,
            onTimeout,
            actionName,
            requiresCombat));
    }

    private static async Task RetryRequestAsync(
        Func<bool> request,
        Func<bool> canRetry,
        Action onTimeout,
        string actionName,
        bool requiresCombat)
    {
        var tree = Engine.GetMainLoop() as SceneTree;
        if (tree is null)
        {
            onTimeout();
            return;
        }

        for (var attempt = 0; attempt < MaxRetryAttempts; attempt++)
        {
            var timer = tree.CreateTimer(RetryDelaySeconds);
            await tree.ToSignal(timer, SceneTreeTimer.SignalName.Timeout);
            if (!canRetry()
                || (requiresCombat && CombatManager.Instance.IsOverOrEnding))
            {
                return;
            }

            if (request())
            {
                return;
            }
        }

        onTimeout();
        Entry.Logger.Warn($"{actionName} managed action request timed out.");
    }
}
