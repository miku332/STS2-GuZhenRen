using GuZhenRen.Cards;
using GuZhenRen.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;

namespace GuZhenRen.Systems;

public enum TribulationType
{
    Earthly,
    Heavenly,
    Grand,
    Myriad,
    MinorChaos,
    MajorChaos
}

public sealed record TribulationDefinition(
    string Id,
    string Name,
    TribulationType Type,
    Func<PlayerTribulationPower, Task> OnCombatStart,
    Func<PlayerTribulationPower, Task> OnPlayerTurnStart);

public static class TribulationSystem
{
    private static readonly IReadOnlyList<TribulationDefinition> Definitions =
    [
        new(
            "DI_ZAI_ZHUO_SHANG",
            "灼伤",
            TribulationType.Earthly,
            AddBurnToHand,
            AddBurnToHand),
        new(
            "DI_ZAI_CHI_TONG_HUO_YI",
            "赤铜火蚁",
            TribulationType.Earthly,
            power => ApplyToRandomEnemy<ChiTongHuoYiPower>(power, 4),
            NoEffect),
        new(
            "DI_ZAI_HUO_XI_NI",
            "和稀泥",
            TribulationType.Earthly,
            power => ApplyToRandomEnemy<HuoXiNiPower>(power, 1),
            NoEffect),
        new(
            "DI_ZAI_ZI_AI",
            "自爱",
            TribulationType.Earthly,
            power => ApplyToRandomEnemy<ZiAiPower>(power, 1),
            NoEffect),
        new(
            "DI_ZAI_SHUI_MU_TIAN_HUA_GU",
            "水幕天华蛊",
            TribulationType.Earthly,
            ApplyShuiMuTianHuaGu,
            NoEffect),
        new(
            "DI_ZAI_LANG_JING",
            "浪静",
            TribulationType.Earthly,
            power => ApplyToRandomEnemy<LangJingPower>(power, 1),
            NoEffect),
        new(
            "DI_ZAI_HUN_BAO_GU",
            "魂爆蛊",
            TribulationType.Earthly,
            ApplyHunBaoGu,
            NoEffect),
        new(
            "TIAN_JIE_HUAN_MAN",
            "缓慢",
            TribulationType.Heavenly,
            ApplySlow,
            NoEffect),
        new(
            "TIAN_JIE_ZHENG_CHANG",
            "正常",
            TribulationType.Heavenly,
            power => ApplyToRandomEnemy<ZhengChangPower>(power, 3),
            NoEffect),
        new(
            "TIAN_JIE_TIE_BI",
            "铁壁",
            TribulationType.Heavenly,
            power => ApplyToRandomEnemy<TieBiPower>(power, 12),
            NoEffect),
        new(
            "TIAN_JIE_SI_QI_JIANG_ZHI",
            "死期将至",
            TribulationType.Heavenly,
            power => ApplyToRandomEnemy<SiQiJiangZhiPower>(power, 6),
            NoEffect),
        new(
            "TIAN_JIE_MU_MEI_GU",
            "木魅蛊",
            TribulationType.Heavenly,
            power => ApplyToRandomEnemy<MuMeiGuPower>(power, 1),
            NoEffect),
        new(
            "TIAN_JIE_HONG_LEI_GU",
            "轰雷蛊",
            TribulationType.Heavenly,
            power => ApplyToRandomEnemy<HongLeiGuPower>(power, 3),
            NoEffect),
        new(
            "TIAN_JIE_XIAO_JIA_ZI_QI",
            "小家子气",
            TribulationType.Heavenly,
            power => ApplyToRandomEnemy<XiaoJiaZiQiPower>(power, 1),
            NoEffect),
        new(
            "TIAN_JIE_CAO_MANG",
            "草莽",
            TribulationType.Heavenly,
            power => ApplyToRandomEnemy<CaoMangPower>(power, 7),
            NoEffect),
        new(
            "HAO_JIE_DING_KONG",
            "定空",
            TribulationType.Grand,
            power => ApplyToRandomEnemy<DingKongPower>(power, 1),
            NoEffect),
        new(
            "HAO_JIE_GUO_DE_QU",
            "过得去",
            TribulationType.Grand,
            power => ApplyToRandomEnemy<GuoDeQuPower>(power, 1),
            NoEffect),
        new(
            "HAO_JIE_GUAN",
            "关",
            TribulationType.Grand,
            power => ApplyToRandomEnemy<GuanPower>(power, 1),
            NoEffect),
        new(
            "HAO_JIE_SONG_ZHEN",
            "松针",
            TribulationType.Grand,
            power => ApplyToRandomEnemy<SongZhenPower>(power, 2),
            NoEffect),
        new(
            "HAO_JIE_YING_SHENG_CHONG",
            "应声虫",
            TribulationType.Grand,
            power => ApplyToRandomEnemy<YingShengChongPower>(power, 1),
            NoEffect),
        new(
            "HAO_JIE_GUI_GUA_YI",
            "鬼卦衣",
            TribulationType.Grand,
            ApplyGuiGuaYi,
            NoEffect),
        new(
            "HAO_JIE_DA_JIA_ZHI_QI",
            "大家之气",
            TribulationType.Grand,
            power => ApplyToRandomEnemy<DaJiaZhiQiPower>(power, 1),
            NoEffect),
        new(
            "HAO_JIE_ZHEN_SUO",
            "镇锁",
            TribulationType.Grand,
            ApplyZhenSuo,
            NoEffect),
        new(
            "HAO_JIE_XU_KONG",
            "虚空",
            TribulationType.Grand,
            AddVoidToDrawPile,
            AddVoidToDrawPile),
        new(
            "WAN_JIE_ZHEN_YU",
            "镇宇",
            TribulationType.Myriad,
            power => ApplyToRandomEnemy<ZhenYuPower>(power, 1),
            NoEffect),
        new(
            "WAN_JIE_LEI_DIAN_GU",
            "雷电蛊",
            TribulationType.Myriad,
            power => ApplyToRandomEnemy<LeiDianGuPower>(power, 3),
            NoEffect),
        new(
            "WAN_JIE_MING_JIA",
            "命甲",
            TribulationType.Myriad,
            power => ApplyToRandomEnemy<MingJiaPower>(power, 1),
            NoEffect),
        new(
            "WAN_JIE_TIAN_WANG",
            "天网",
            TribulationType.Myriad,
            power => ApplyToRandomEnemy<TianWangPower>(power, 3),
            NoEffect),
        new(
            "WAN_JIE_CHOU_HEN_GU",
            "仇恨蛊",
            TribulationType.Myriad,
            power => ApplyToRandomEnemy<ChouHenGuPower>(power, 1),
            NoEffect),
        new(
            "WAN_JIE_TONG_XIN",
            "通心",
            TribulationType.Myriad,
            power => ApplyToRandomEnemy<TongXinPower>(power, 1),
            NoEffect),
        new(
            "WAN_JIE_DOU_ZHUAN",
            "斗转",
            TribulationType.Myriad,
            power => ApplyToRandomEnemy<DouZhuanPower>(power, 1),
            NoEffect),
        new(
            "HUN_DUN_XIAO_NAN_HEI_HUO",
            "混沌小难",
            TribulationType.MinorChaos,
            AddBlackFireToDrawPile,
            AddBlackFireToDrawPile),
        new(
            "HUN_DUN_DA_NAN_HUN_DUN",
            "混沌",
            TribulationType.MajorChaos,
            AddChaosToDrawPile,
            AddChaosToDrawPile)
    ];

    public static TribulationType GetNextType(int rank, int progress)
    {
        if (rank <= 5)
        {
            return TribulationType.Earthly;
        }

        return rank switch
        {
            6 => progress == 0
                ? TribulationType.Earthly
                : TribulationType.Heavenly,
            7 => progress == 0
                ? TribulationType.Heavenly
                : TribulationType.Grand,
            8 => progress switch
            {
                0 => TribulationType.Grand,
                1 => TribulationType.Myriad,
                _ => TribulationType.MinorChaos
            },
            _ => (progress % 6) switch
            {
                < 3 => TribulationType.Grand,
                < 5 => TribulationType.Myriad,
                _ => TribulationType.MajorChaos
            }
        };
    }

    public static int GetTypeIndex(TribulationType type) => (int)type;

    public static TribulationDefinition Select(
        TribulationType type,
        Player player)
    {
        var candidates = Definitions.Where(definition =>
            definition.Type == type).ToList();

        if (candidates.Count == 0)
        {
            throw new InvalidOperationException(
                $"No tribulation is registered for {type}.");
        }

        return player.RunState.Rng.CombatCardSelection.NextItem(candidates)!;
    }

    private static Task NoEffect(PlayerTribulationPower power) =>
        Task.CompletedTask;

    private static async Task ApplyToRandomEnemy<T>(
        PlayerTribulationPower power,
        int amount)
        where T : PowerModel
    {
        if (power.Owner.Player is not { } player
            || power.Owner.CombatState is not { } combatState)
        {
            return;
        }

        var candidates = combatState.Enemies
            .Where(enemy => enemy.IsAlive
                && enemy.GetPower<MinionPower>() is null)
            .ToList();
        if (candidates.Count == 0)
        {
            return;
        }

        var target = player.RunState.Rng.CombatTargets.NextItem(candidates)!;
        power.FlashEffect();
        await PowerCmd.Apply<T>(
            new ThrowingPlayerChoiceContext(),
            target,
            amount,
            power.Owner,
            null);
    }

    private static async Task ApplyHunBaoGu(PlayerTribulationPower power)
    {
        if (power.Owner.CombatState is not { } combatState)
        {
            return;
        }

        var targets = combatState.Enemies
            .Where(enemy => enemy.IsAlive
                && enemy.GetPower<MinionPower>() is null)
            .ToList();
        if (targets.Count == 0)
        {
            return;
        }

        var amount = targets.Count switch
        {
            1 => 20,
            2 => 14,
            3 => 10,
            _ => 8
        };

        power.FlashEffect();
        foreach (var target in targets)
        {
            await PowerCmd.Apply<HunBaoGuPower>(
                new ThrowingPlayerChoiceContext(),
                target,
                amount,
                power.Owner,
                null);
        }
    }

    private static async Task ApplyShuiMuTianHuaGu(
        PlayerTribulationPower power)
    {
        if (power.Owner.Player is not { } player
            || power.Owner.CombatState is not { } combatState)
        {
            return;
        }

        var candidates = combatState.Enemies
            .Where(enemy => enemy.IsAlive
                && enemy.GetPower<MinionPower>() is null)
            .ToList();
        if (candidates.Count == 0)
        {
            return;
        }

        var target = player.RunState.Rng.CombatTargets.NextItem(candidates)!;
        var amount = target.GetPower<BarricadePower>() is null ? 25 : 20;
        power.FlashEffect();
        await PowerCmd.Apply<ShuiMuTianHuaGuPower>(
            new ThrowingPlayerChoiceContext(),
            target,
            amount,
            power.Owner,
            null);
    }

    private static async Task ApplyGuiGuaYi(PlayerTribulationPower power)
    {
        if (power.Owner.CombatState is not { } combatState)
        {
            return;
        }

        var targets = combatState.Enemies
            .Where(enemy => enemy.IsAlive
                && enemy.GetPower<MinionPower>() is null)
            .ToList();
        if (targets.Count == 0)
        {
            return;
        }

        power.FlashEffect();
        foreach (var target in targets)
        {
            await PowerCmd.Apply<GuiGuaYiPower>(
                new ThrowingPlayerChoiceContext(),
                target,
                1,
                power.Owner,
                null);
        }
    }

    private static async Task ApplyZhenSuo(PlayerTribulationPower power)
    {
        if (power.Owner.Player is not { } player
            || power.Owner.CombatState is not { } combatState)
        {
            return;
        }

        var targets = combatState.Enemies
            .Where(enemy => enemy.IsAlive
                && enemy.GetPower<MinionPower>() is null)
            .ToList();
        if (targets.Count == 0)
        {
            return;
        }

        power.FlashEffect();
        var zhenTarget = player.RunState.Rng.CombatTargets.NextItem(targets)!;
        var remaining = targets.Where(target => target != zhenTarget).ToList();
        var suoTarget = remaining.Count > 0
            ? player.RunState.Rng.CombatTargets.NextItem(remaining)!
            : zhenTarget;

        await PowerCmd.Apply<ZhenPower>(
            new ThrowingPlayerChoiceContext(),
            zhenTarget,
            1,
            power.Owner,
            null);
        await PowerCmd.Apply<SuoPower>(
            new ThrowingPlayerChoiceContext(),
            suoTarget,
            1,
            power.Owner,
            null);
    }

    private static async Task AddBurnToHand(PlayerTribulationPower power)
    {
        if (power.Owner.Player is not { } player
            || power.Owner.CombatState is null)
        {
            return;
        }

        power.FlashEffect();
        var burn = power.Owner.CombatState.CreateCard<Burn>(player);
        await CardPileCmd.AddGeneratedCardToCombat(
            burn,
            PileType.Hand,
            player,
            CardPilePosition.Bottom);
    }

    private static async Task ApplySlow(PlayerTribulationPower power)
    {
        if (power.Owner.Player is null)
        {
            return;
        }

        power.FlashEffect();
        await PowerCmd.Apply<SlowPower>(
            new ThrowingPlayerChoiceContext(),
            power.Owner,
            1,
            power.Owner,
            null);
    }

    private static async Task AddVoidToDrawPile(PlayerTribulationPower power)
    {
        if (power.Owner.Player is not { } player
            || power.Owner.CombatState is null)
        {
            return;
        }

        power.FlashEffect();
        var card = power.Owner.CombatState.CreateCard<MegaCrit.Sts2.Core.Models.Cards.Void>(player);
        await CardPileCmd.AddGeneratedCardToCombat(
            card,
            PileType.Draw,
            player,
            CardPilePosition.Bottom);
    }

    private static async Task AddBlackFireToDrawPile(
        PlayerTribulationPower power)
    {
        if (power.Owner.Player is not { } player
            || power.Owner.CombatState is null)
        {
            return;
        }

        power.FlashEffect();
        var card = power.Owner.CombatState.CreateCard<HeiHuo>(player);
        await CardPileCmd.AddGeneratedCardToCombat(
            card,
            PileType.Draw,
            player,
            CardPilePosition.Bottom);
    }

    private static async Task AddChaosToDrawPile(
        PlayerTribulationPower power)
    {
        if (power.Owner.Player is not { } player
            || power.Owner.CombatState is null)
        {
            return;
        }

        power.FlashEffect();
        var card = power.Owner.CombatState.CreateCard<HunDun>(player);
        await CardPileCmd.AddGeneratedCardToCombat(
            card,
            PileType.Draw,
            player,
            CardPilePosition.Bottom);
    }
}
