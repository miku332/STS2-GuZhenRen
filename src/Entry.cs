using System.Reflection;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using STS2RitsuLib;
using STS2RitsuLib.Interop;
using GuZhenRen.Cards;

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
        if (_lifecycleSubscriptions.Count == 0)
        {
            _lifecycleSubscriptions.Add(RitsuLibFramework.SubscribeLifecycle<CardDrawnEvent>(
                static evt =>
                {
                    if (evt.FromHandDraw && evt.Card is ShiZhen shiZhen)
                    {
                        shiZhen.OnCardDrawn();
                    }

                    if (evt.FromHandDraw)
                    {
                        XueKuangGu.RefreshCachedAdjacentCardsInHand(evt.Card.Owner);
                    }

                    XueKuangGu.TryAutoPlayBloodcrazedCard(evt.Card);
                },
                replayCurrentState: false));
            _lifecycleSubscriptions.Add(RitsuLibFramework.SubscribeLifecycle<CardMovedBetweenPilesEvent>(
                static evt =>
                {
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
                static _ => XueKuangGu.ClearCombatState(),
                replayCurrentState: false));
        }

        Logger.Info("Gu Zhen Ren mod initialized.");
    }
}
