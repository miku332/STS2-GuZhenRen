using System.Reflection;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib;
using STS2RitsuLib.Interop;
using GuZhenRen.Cards;
using STS2RitsuLib.Patching.Core;
using GuZhenRen.Patches;
using GuZhenRen.Powers;
using GuZhenRen.Systems;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Rooms;

namespace GuZhenRen;

[ModInitializer(nameof(Init))]
public static class Entry
{
    public const string ModId = "GuZhenRen";

    public static readonly Logger Logger = RitsuLibFramework.CreateLogger(ModId);

    private static readonly List<IDisposable> _lifecycleSubscriptions = [];

    public static void Init()
    {
        var assembly = Assembly.GetExecutingAssembly();

        RitsuLibFramework.EnsureGodotScriptsRegistered(assembly, Logger);
        ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);
        var tunHuoPatcher = RitsuLibFramework.CreatePatcher(ModId, "tun-huo");
        tunHuoPatcher.RegisterPatch<TunHuoPatch>();
        tunHuoPatcher.PatchAll();
        var huaShaPatcher = RitsuLibFramework.CreatePatcher(ModId, "hua-sha");
        huaShaPatcher.RegisterPatch<HuaShaLoseBlockPatch>();
        huaShaPatcher.RegisterPatch<HuaShaClearBlockPatch>();
        huaShaPatcher.PatchAll();
        var benMingGuPatcher = RitsuLibFramework.CreatePatcher(ModId, "ben-ming-gu");
        benMingGuPatcher.RegisterPatch<BenMingGuUniquenessPatch>();
        benMingGuPatcher.RegisterPatch<XianGuCanHaiSmithRestSitePatch>();
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
        if (_lifecycleSubscriptions.Count == 0)
        {
            _lifecycleSubscriptions.Add(RitsuLibFramework.SubscribeLifecycle<CardDrawnEvent>(
                static evt =>
                {
                    if (evt.FromHandDraw && evt.Card is ShiZhen shiZhen)
                    {
                        shiZhen.OnCardDrawn();
                    }

                    if (evt.Card is DiMai diMai)
                    {
                        diMai.OnCardDrawn();
                    }

                    if (evt.Card is EYun eyun)
                    {
                        eyun.OnCardDrawn();
                    }

                    if (evt.FromHandDraw && evt.Card is AiQingGu aiQingGu)
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
                                AbstractXianGuWuCard.ReturnAllToHand(player));
                        }
                    }
                },
                replayCurrentState: false));
            _lifecycleSubscriptions.Add(RitsuLibFramework.SubscribeLifecycle<AttackEndedEvent>(
                static evt => ZhuiMingHuoPower.AfterAttackEnded(evt),
                replayCurrentState: false));
            _lifecycleSubscriptions.Add(RitsuLibFramework.SubscribeLifecycle<CreatureDiedEvent>(
                static evt => ShaGu.AfterCreatureDied(evt),
                replayCurrentState: false));
            _lifecycleSubscriptions.Add(RitsuLibFramework.SubscribeLifecycle<RoomEnteredEvent>(
                static evt =>
                {
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
