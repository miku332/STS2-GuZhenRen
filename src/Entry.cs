using System.Reflection;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib;
using STS2RitsuLib.Interop;
using GuZhenRen.Cards;
using GuZhenRen.Potions;
using STS2RitsuLib.Patching.Core;
using GuZhenRen.Patches;
using GuZhenRen.Powers;
using GuZhenRen.Relics;
using GuZhenRen.Systems;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Rooms;

namespace GuZhenRen;

[ModInitializer(nameof(Init))]
public static class Entry
{
    public const string ModId = "GuZhenRen";
    public const string Version = "0.4.10-beta.1";

    public static readonly Logger Logger = RitsuLibFramework.CreateLogger(ModId);

    private static readonly List<IDisposable> _lifecycleSubscriptions = [];
    private static IDisposable? _updateCheckRegistration;

    public static void Init()
    {
        var assembly = Assembly.GetExecutingAssembly();

        RitsuLibFramework.EnsureGodotScriptsRegistered(assembly, Logger);
        ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);
        _updateCheckRegistration ??= ModUpdateSystem.Register(assembly);
        RitsuLibFramework.RegisterArchaicToothTranscendenceMapping<XiaoGuangGu, TaiChuGuangGu>(ModId);
        var tunHuoPatcher = RitsuLibFramework.CreatePatcher(ModId, "tun-huo");
        tunHuoPatcher.RegisterPatch<TunHuoPatch>();
        tunHuoPatcher.PatchAll();
        var yongShengPatcher = RitsuLibFramework.CreatePatcher(
            ModId,
            "yong-sheng");
        yongShengPatcher.RegisterPatch<YongShengLoseMaxHpPatch>();
        yongShengPatcher.RegisterPatch<YongShengKillPatch>();
        yongShengPatcher.PatchAll();
        var guiBuJuePatcher = RitsuLibFramework.CreatePatcher(
            ModId,
            "gui-bu-jue");
        guiBuJuePatcher.RegisterPatch<GuiBuJueMoveTargetPatch>();
        guiBuJuePatcher.RegisterPatch<GuiBuJueAttackTargetPatch>();
        guiBuJuePatcher.PatchAll();
        var jianChiGuPatcher = RitsuLibFramework.CreatePatcher(
            ModId,
            "jian-chi-gu");
        jianChiGuPatcher.RegisterPatch<JianChiGuDurationPatch>();
        jianChiGuPatcher.RegisterPatch<JianChiGuDecrementPatch>();
        jianChiGuPatcher.PatchAll();
        var huaShaPatcher = RitsuLibFramework.CreatePatcher(ModId, "hua-sha");
        huaShaPatcher.RegisterPatch<HuaShaLoseBlockPatch>();
        huaShaPatcher.RegisterPatch<HuaShaClearBlockPatch>();
        huaShaPatcher.PatchAll();
        var benMingGuPatcher = RitsuLibFramework.CreatePatcher(ModId, "ben-ming-gu");
        benMingGuPatcher.RegisterPatch<BenMingGuUniquenessPatch>();
        benMingGuPatcher.RegisterPatch<XianGuUpgradeUniquenessPatch>();
        benMingGuPatcher.RegisterPatch<BenMingGuRemovalPenaltyPatch>();
        benMingGuPatcher.RegisterPatch<BenMingGuSelectionHeaderPatch>();
        benMingGuPatcher.RegisterPatch<BenMingGuPersistentDowngradePatch>();
        benMingGuPatcher.RegisterPatch<ReflectionsBenMingGuPatch>();
        benMingGuPatcher.RegisterPatch<WongoBenMingGuPatch>();
        benMingGuPatcher.RegisterPatch<XianGuCanHaiSmithRestSitePatch>();
        benMingGuPatcher.RegisterPatch<CunGuangYinSmithPatch>();
        benMingGuPatcher.RegisterPatch<CunGuangYinSmithCountPatch>();
        benMingGuPatcher.PatchAll();
        var aiQingGuPatcher = RitsuLibFramework.CreatePatcher(ModId, "ai-qing-gu");
        aiQingGuPatcher.RegisterPatch<AiQingGuEscapeRewardPatch>();
        aiQingGuPatcher.PatchAll();
        var niLiuHePatcher = RitsuLibFramework.CreatePatcher(ModId, "ni-liu-he");
        niLiuHePatcher.RegisterPatch<NiLiuHeDamagePatch>();
        niLiuHePatcher.RegisterPatch<NiLiuHePowerApplyPatch>();
        niLiuHePatcher.RegisterPatch<NiLiuHePowerLookupPatch>();
        niLiuHePatcher.RegisterPatch<NiLiuHePowerModifyPatch>();
        niLiuHePatcher.PatchAll();
        var cardDisplayPatcher = RitsuLibFramework.CreatePatcher(
            ModId,
            "card-display");
        cardDisplayPatcher.RegisterPatch<CardRankDescriptionPatch>();
        cardDisplayPatcher.PatchAll();
        var shaZhaoPoolPatcher = RitsuLibFramework.CreatePatcher(
            ModId,
            "sha-zhao-pools");
        shaZhaoPoolPatcher.RegisterPatch<ShaZhaoRewardPoolPatch>();
        shaZhaoPoolPatcher.RegisterPatch<ShaZhaoMerchantPoolPatch>();
        shaZhaoPoolPatcher.RegisterPatch<XianGuCardRewardResultPatch>();
        shaZhaoPoolPatcher.PatchAll();
        var monsterCompatibilityPatcher = RitsuLibFramework.CreatePatcher(
            ModId,
            "monster-compatibility");
        monsterCompatibilityPatcher.RegisterPatch<CeremonialBeastStunPatch>();
        monsterCompatibilityPatcher.PatchAll();
        var potionPatcher = RitsuLibFramework.CreatePatcher(ModId, "potion-state");
        potionPatcher.RegisterPatch<FuRenXinPotionRemovalPatch>();
        potionPatcher.PatchAll();
        var liQiPatcher = RitsuLibFramework.CreatePatcher(ModId, "li-qi");
        liQiPatcher.RegisterPatch<XuYingHandSizePatch>();
        liQiPatcher.PatchAll();
        var haoJiePatcher = RitsuLibFramework.CreatePatcher(ModId, "hao-jie");
        haoJiePatcher.RegisterPatch<GuiGuaYiIntentPatch>();
        haoJiePatcher.PatchAll();
        var orobasPatcher = RitsuLibFramework.CreatePatcher(ModId, "orobas-fang-yuan");
        orobasPatcher.RegisterPatch<OrobasFangYuanSetupPatch>();
        orobasPatcher.RegisterPatch<OrobasFangYuanObtainPatch>();
        orobasPatcher.RegisterPatch<OrobasFangYuanUpgradePatch>();
        orobasPatcher.RegisterPatch<XianTaiGuTransferPatch>();
        orobasPatcher.RegisterPatch<ArchaicToothFangYuanSetupPatch>();
        orobasPatcher.PatchAll();
        var tezcataraRelicPatcher = RitsuLibFramework.CreatePatcher(ModId, "tezcatara-relic-pool");
        tezcataraRelicPatcher.RegisterPatch<TezcataraRelicPoolPatch>();
        tezcataraRelicPatcher.PatchAll();
        var eventAssetPatcher = RitsuLibFramework.CreatePatcher(ModId, "event-assets");
        eventAssetPatcher.RegisterPatch<EventPortraitPreloadPatch>();
        eventAssetPatcher.PatchAll();
        var shaZhaoUiPatcher = RitsuLibFramework.CreatePatcher(
            ModId,
            "sha-zhao-ui");
        shaZhaoUiPatcher.RegisterPatch<ShaZhaoRecipeSelectionBackPatch>();
        shaZhaoUiPatcher.RegisterPatch<ShaZhaoRecipeSelectionReplacePatch>();
        shaZhaoUiPatcher.RegisterPatch<KillerMoveRelicRowPatch>();
        shaZhaoUiPatcher.PatchAll();
        if (_lifecycleSubscriptions.Count == 0)
        {
            _lifecycleSubscriptions.Add(RitsuLibFramework.SubscribeLifecycle<CardDrawnEvent>(
                static evt =>
                {
                    if (evt.Card is DiMai diMai)
                    {
                        diMai.OnCardDrawn();
                    }

                    if (evt.Card is EYun eyun)
                    {
                        eyun.OnCardDrawn();
                    }

                    if (evt.Card is AiQingGu aiQingGu)
                    {
                        aiQingGu.OnCardDrawn();
                    }

                    PaiNanPower.TryHandleCardDrawn(evt.Card);

                    if (evt.FromHandDraw)
                    {
                        XueKuangGu.RefreshCachedAdjacentCardsInHand(evt.Card.Owner);
                    }

                    XueKuangGu.TryAutoPlayBloodcrazedCard(evt.Card);
                },
                replayCurrentState: false));
            _lifecycleSubscriptions.Add(RitsuLibFramework.SubscribeLifecycle<ShuffledEvent>(
                static evt =>
                {
                    foreach (var card in PileType.Draw.GetPile(evt.Shuffler).Cards.ToList())
                    {
                        if (card is DiMai diMai)
                        {
                            diMai.KeepAtDrawPileBottom();
                        }
                    }
                },
                replayCurrentState: false));
            _lifecycleSubscriptions.Add(RitsuLibFramework.SubscribeLifecycle<CardMovedBetweenPilesEvent>(
                static evt =>
                {
                    if (evt.Card.Pile?.Type == PileType.Draw
                        && evt.Card is DiMai diMai)
                    {
                        diMai.KeepAtDrawPileBottom();
                    }

                    if (evt.Card.Pile?.Type == MegaCrit.Sts2.Core.Entities.Cards.PileType.Hand
                        || evt.PreviousPile == MegaCrit.Sts2.Core.Entities.Cards.PileType.Hand)
                    {
                        XueKuangGu.RefreshCachedAdjacentCardsInHand(evt.Card.Owner);
                    }

                    if (evt.Card.Pile?.Type == MegaCrit.Sts2.Core.Entities.Cards.PileType.Hand
                        && evt.PreviousPile != MegaCrit.Sts2.Core.Entities.Cards.PileType.Hand)
                    {
                        XueKuangGu.TryAutoPlayBloodcrazedCard(evt.Card);
                    }
                },
                replayCurrentState: false));
            _lifecycleSubscriptions.Add(RitsuLibFramework.SubscribeLifecycle<CardPlayingEvent>(
                static evt =>
                {
                    if (evt.CardPlay.Card is XueKuangGu xueKuangGu)
                    {
                        xueKuangGu.CacheAdjacentCardsFromHand();
                    }
                },
                replayCurrentState: false));
            _lifecycleSubscriptions.Add(RitsuLibFramework.SubscribeLifecycle<CombatEndedEvent>(
                static evt =>
                {
                    NiLiuHeReflectionState.Clear();
                    XueKuangGu.ClearCombatState();
                    RenRuGu.ResetCombatHistory();
                    AnTuZhongShanBao.ResetCombatState();
                    XingXiuQiPan.ResetCombatState();
                    ZhuMoBang.ResetCombatState();
                    TouDaoDaoHenPower.ResetCombatState();
                    ShaZhaoRecipeDropSystem.TryAddCombatReward(evt);
                    TaskHelper.RunSafely(HandleEyunCombatEnded(evt));
                },
                replayCurrentState: false));
            _lifecycleSubscriptions.Add(RitsuLibFramework.SubscribeLifecycle<CombatStartingEvent>(
                static evt =>
                {
                    NiLiuHeReflectionState.Clear();
                    RenRuGu.ResetCombatHistory();
                    AnTuZhongShanBao.ResetCombatState();
                    XingXiuQiPan.ResetCombatState();
                    ZhuMoBang.ResetCombatState();
                    TouDaoDaoHenPower.ResetCombatState();

                    if (evt.CombatState is not null)
                    {
                        foreach (var player in evt.CombatState.Players)
                        {
                            foreach (var card in PileType.Draw.GetPile(player).Cards.ToList())
                            {
                                if (card is DiMai diMai)
                                {
                                    diMai.KeepAtDrawPileBottom();
                                }
                            }
                        }
                    }
                },
                replayCurrentState: false));
            _lifecycleSubscriptions.Add(RitsuLibFramework.SubscribeLifecycle<SideTurnStartedEvent>(
                static evt =>
                {
                    if (evt.Side == CombatSide.Player)
                    {
                        NiLiuHeReflectionState.Clear();
                        RenRuGu.RecordPlayerTurnStart(evt.CombatState);
                        foreach (var player in evt.CombatState.Players)
                        {
                            TaskHelper.RunSafely(
                                AbstractGuWuCard.ReturnAllToHand(player));
                        }
                    }
                },
                replayCurrentState: false));
            _lifecycleSubscriptions.Add(RitsuLibFramework.SubscribeLifecycle<AttackEndedEvent>(
                static evt => ZhuiMingHuoPower.AfterAttackEnded(evt),
                replayCurrentState: false));
            _lifecycleSubscriptions.Add(RitsuLibFramework.SubscribeLifecycle<CreatureDiedEvent>(
                static evt =>
                {
                    ShaGu.AfterCreatureDied(evt);
                    TaskHelper.RunSafely(ChiXiang.AfterCreatureDied(evt));
                    FuRenXin.AfterCreatureDied(evt);
                },
                replayCurrentState: false));
            _lifecycleSubscriptions.Add(RitsuLibFramework.SubscribeLifecycle<PotionProcuredEvent>(
                static evt =>
                {
                    if (evt.Potion is FuRenXin fuRenXin)
                    {
                        FuRenXin.OnProcured(fuRenXin);
                    }
                },
                replayCurrentState: false));
            _lifecycleSubscriptions.Add(RitsuLibFramework.SubscribeLifecycle<RoomEnteredEvent>(
                static evt =>
                {
                    foreach (var player in evt.RunState.Players)
                    {
                        BenMingGuRankProtection.EnsureMinimumRank(player);
                        TaskHelper.RunSafely(
                            BenMingGuUniquenessPatch.EnforceDeckUniqueness(player));
                    }

                    TaskHelper.RunSafely(
                        BenMingGuSelectionCoordinator.TrySelect(evt.Room));

                    if (evt.Room is MerchantRoom)
                    {
                        TaskHelper.RunSafely(
                            GuQiangGuShopExchange.ExchangeAll(evt.RunState));
                    }
                },
                replayCurrentState: false));
        }

        Logger.Info("Gu Zhen Ren mod initialized.");
    }

    private static async Task HandleEyunCombatEnded(CombatEndedEvent evt)
    {
        foreach (var player in evt.RunState.Players)
        {
            foreach (var card in player.Deck.Cards.OfType<EYun>().ToList())
            {
                await card.OnCombatEnded();
            }
        }
    }
}
