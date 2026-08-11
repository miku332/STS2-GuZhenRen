using GuZhenRen.CardPools;
using GuZhenRen.Relics;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Events;

[RegisterActEvent(typeof(Glory))]
public sealed class DuShiChang : ModEventTemplate
{
    private const int BaseChance = 5;
    private const int GouShiYunBonus = 5;
    private const int HongYunQiTianGuBonus = 10;
    private const int CommonCost = 5;
    private const int UncommonCost = 10;
    private const int RareCost = 15;
    private const int MaxWins = 3;

    private int _chance = BaseChance;
    private int _commonWins;
    private int _uncommonWins;
    private int _rareWins;
    private List<RelicModel> _commonCandidates = [];
    private List<RelicModel> _uncommonCandidates = [];
    private List<RelicModel> _rareCandidates = [];

    public override EventAssetProfile AssetProfile => new(
        InitialPortraitPath: "res://GuZhenRen/images/events/DuShiChang.png");

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Chance", BaseChance),
        new DynamicVar("CommonCost", CommonCost),
        new DynamicVar("UncommonCost", UncommonCost),
        new DynamicVar("RareCost", RareCost),
        new DynamicVar("CommonWins", 0),
        new DynamicVar("UncommonWins", 0),
        new DynamicVar("RareWins", 0),
        new DynamicVar("MaxWins", MaxWins)
    ];

    // Match the original mod: ranks 2-5 and at least 30 gold.
    public override bool IsAllowed(IRunState runState) =>
        runState.Players.All(player =>
            player.GetRelic<AbstractKongQiaoRelic>() is { Rank: > 1 and <= 5 } &&
            player.Gold >= 30);

    protected override Task BeforeEventStarted(bool isPreFinished)
    {
        _chance = GetChance();
        _commonCandidates = GetAvailableCandidates<JianMei, ShuiWenGu, TuDuiGu>();
        _uncommonCandidates = GetAvailableCandidates<ChiXiang, FengXiongHuaJi, GouShiYun, NongXuGu, SiXuRuDianGu, YanXinGu>();
        _rareCandidates = GetAvailableCandidates<CunGuangYin, FeiLiGu, HongYunQiTianGu, MuYa, NengLiGu, TouSheng>();
        RefreshDynamicVars();
        return Task.CompletedTask;
    }

    protected override IReadOnlyList<EventOption> GenerateInitialOptions() =>
        BuildOptions("INITIAL");

    private IReadOnlyList<EventOption> BuildOptions(string pageName) =>
    [
        CreateCounterOption(
            pageName,
            "COMMON",
            CommonCost,
            _commonWins,
            _commonCandidates,
            GambleCommon),
        CreateCounterOption(
            pageName,
            "UNCOMMON",
            UncommonCost,
            _uncommonWins,
            _uncommonCandidates,
            GambleUncommon),
        CreateCounterOption(
            pageName,
            "RARE",
            RareCost,
            _rareWins,
            _rareCandidates,
            GambleRare),
        new EventOption(this, Leave, ModOptionKey(pageName, "LEAVE"))
    ];

    private EventOption CreateCounterOption(
        string pageName,
        string optionName,
        int cost,
        int wins,
        IReadOnlyCollection<RelicModel> candidates,
        Func<Task> gamble)
    {
        var key = wins >= MaxWins || candidates.Count == 0
            ? ModOptionKey(pageName, $"{optionName}_SOLD_OUT")
            : Owner!.Gold < cost
                ? ModOptionKey(pageName, $"{optionName}_NO_GOLD")
                : ModOptionKey(pageName, optionName);
        return new EventOption(
            this,
            wins >= MaxWins || candidates.Count == 0 || Owner!.Gold < cost ? null : gamble,
            key);
    }

    private Task GambleCommon() => Gamble(
        CommonCost,
        _commonCandidates,
        () => _commonWins++,
        "COMMON_SUCCESS");

    private Task GambleUncommon() => Gamble(
        UncommonCost,
        _uncommonCandidates,
        () => _uncommonWins++,
        "UNCOMMON_SUCCESS");

    private Task GambleRare() => Gamble(
        RareCost,
        _rareCandidates,
        () => _rareWins++,
        "RARE_SUCCESS");

    private async Task Gamble(
        int cost,
        List<RelicModel> candidates,
        Action onSuccess,
        string successPage)
    {
        await PlayerCmd.LoseGold(cost, Owner!, GoldLossType.Spent);

        if (Owner!.PlayerRng.Rewards.NextFloat(100f) >= _chance)
        {
            SetEventState(
                L10NLookup($"{Id.Entry}.pages.FAILURE.description"),
                BuildOptions("FAILURE"));
            return;
        }

        var canonical = Owner.RunState.Rng.UpFront.NextItem(candidates);
        if (canonical is null)
        {
            SetEventState(
                L10NLookup($"{Id.Entry}.pages.FAILURE.description"),
                BuildOptions("FAILURE"));
            return;
        }

        candidates.Remove(canonical);
        onSuccess();
        RefreshDynamicVars();
        await RelicCmd.Obtain(canonical.ToMutable(), Owner);
        SetEventState(
            L10NLookup($"{Id.Entry}.pages.{successPage}.description"),
            BuildOptions(successPage));
    }

    private Task Leave()
    {
        SetEventFinished(L10NLookup($"{Id.Entry}.pages.LEAVE.description"));
        return Task.CompletedTask;
    }

    private int GetChance()
    {
        var chance = BaseChance;
        if (Owner!.GetRelic<GouShiYun>() is not null)
        {
            chance += GouShiYunBonus;
        }

        if (Owner.GetRelic<HongYunQiTianGu>() is not null)
        {
            chance += HongYunQiTianGuBonus;
        }

        return chance;
    }

    private void RefreshDynamicVars()
    {
        DynamicVars["Chance"].BaseValue = _chance;
        DynamicVars["CommonWins"].BaseValue = _commonWins;
        DynamicVars["UncommonWins"].BaseValue = _uncommonWins;
        DynamicVars["RareWins"].BaseValue = _rareWins;
    }

    private List<RelicModel> GetAvailableCandidates<TCommon1, TCommon2, TCommon3>()
        where TCommon1 : RelicModel
        where TCommon2 : RelicModel
        where TCommon3 : RelicModel =>
        GetAvailableCandidates(
            ModelDb.Relic<TCommon1>(),
            ModelDb.Relic<TCommon2>(),
            ModelDb.Relic<TCommon3>());

    private List<RelicModel> GetAvailableCandidates<T1, T2, T3, T4, T5, T6>()
        where T1 : RelicModel
        where T2 : RelicModel
        where T3 : RelicModel
        where T4 : RelicModel
        where T5 : RelicModel
        where T6 : RelicModel =>
        GetAvailableCandidates(
            ModelDb.Relic<T1>(),
            ModelDb.Relic<T2>(),
            ModelDb.Relic<T3>(),
            ModelDb.Relic<T4>(),
            ModelDb.Relic<T5>(),
            ModelDb.Relic<T6>());

    private List<RelicModel> GetAvailableCandidates(params RelicModel[] candidates) =>
        candidates
            .Where(candidate => Owner!.Relics.All(owned => owned.Id != candidate.Id))
            .ToList();
}
