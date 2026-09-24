using GuZhenRen.Characters;
using GuZhenRen.Multiplayer;
using GuZhenRen.Powers;
using GuZhenRen.Relics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib;
using STS2RitsuLib.Networking.ManagedActions;

namespace GuZhenRen.Systems;

internal static class ShaZhaoRecipeDropSystem
{
    private const float MonsterDropChance = 15f;
    private const float EliteDropChance = 50f;
    private const float BossDropChance = 100f;
    private const int NoTribulation = -1;

    public static void TryAddCombatReward(CombatEndedEvent evt)
    {
        if (evt.Room is not CombatRoom room || !room.Encounter.ShouldGiveRewards)
        {
            return;
        }

        if (!MultiplayerActionAuthority.IsAuthority)
        {
            return;
        }

        var payload = new ShaZhaoRecipeRewardsPayload(
            room.CombatState.Players
                .Select(player => new ShaZhaoRecipeTribulationPayload(
                    player.NetId,
                    GetCombatTribulationType(player)))
                .ToArray());

        NetGuZhenRenActions.RequestWithRetry(
            () => NetGuZhenRenActions.RequestShaZhaoRecipeRewards(payload),
            () => evt.RunState.CurrentRoom == room,
            () => Entry.Logger.Warn(
                "Sha Zhao recipe reward action was not queued."),
            "Sha Zhao recipe rewards",
            requiresCombat: false);
    }

    internal static Task ExecuteManagedRewardsAsync(
        RitsuLibManagedNetActionContext<ShaZhaoRecipeRewardsPayload> context)
    {
        if (context.Player.RunState.CurrentRoom is CombatRoom room
            && room.Encounter.ShouldGiveRewards)
        {
            AddCombatRewards(room, context.Message);
        }

        return Task.CompletedTask;
    }

    private static void AddCombatRewards(
        CombatRoom room,
        ShaZhaoRecipeRewardsPayload payload)
    {
        foreach (var player in room.CombatState.Players)
        {
            if (player.Character is not FangYuanCharacter)
            {
                continue;
            }

            var tribulationType = GetPayloadTribulationType(payload, player);
            TryAddRecipeReward(room, player, tribulationType);
            TryAddTribulationRelicReward(room, player, tribulationType);
        }
    }

    private static void TryAddRecipeReward(
        CombatRoom room,
        Player player,
        TribulationType? tribulationType)
    {
        if (HasRecipeReward(room, player))
        {
            return;
        }

        var candidates = GetUnownedRecipes(player);
        if (candidates.Count == 0)
        {
            Entry.Logger.Info(
                "Sha Zhao recipe reward skipped: every recipe is already owned.");
            return;
        }

        var chance = GetRecipeDropChance(room.RoomType, tribulationType);
        if (player.PlayerRng.Rewards.NextFloat(100f) >= chance)
        {
            Entry.Logger.Info(
                $"Sha Zhao recipe reward missed: {chance:0}% chance in {room.RoomType} room"
                + GetTribulationLogSuffix(tribulationType) + ".");
            return;
        }

        var canonicalRecipe = player.PlayerRng.Rewards.NextItem(candidates);
        if (canonicalRecipe is null)
        {
            return;
        }

        room.AddExtraReward(
            player,
            new RelicReward(canonicalRecipe.ToMutable(), player));
        Entry.Logger.Info(
            $"Added Sha Zhao recipe reward {canonicalRecipe.Id.Entry} "
            + $"from {room.RoomType} room at {chance:0}% chance"
            + GetTribulationLogSuffix(tribulationType) + ".");
    }

    private static void TryAddTribulationRelicReward(
        CombatRoom room,
        Player player,
        TribulationType? tribulationType)
    {
        var chance = GetExtraRelicDropChance(tribulationType);
        if (chance <= 0f
            || player.PlayerRng.Rewards.NextFloat(100f) >= chance)
        {
            return;
        }

        room.AddExtraReward(player, new RelicReward(player));
        Entry.Logger.Info(
            $"Added tribulation relic reward at {chance:0}% chance"
            + GetTribulationLogSuffix(tribulationType) + ".");
    }

    private static float GetRecipeDropChance(
        RoomType roomType,
        TribulationType? tribulationType)
    {
        var baseChance = roomType switch
        {
            RoomType.Elite => EliteDropChance,
            RoomType.Boss => BossDropChance,
            _ => MonsterDropChance
        };

        var multiplier = tribulationType switch
        {
            TribulationType.Earthly => 3f,
            TribulationType.Heavenly => 5f,
            TribulationType.Grand
                or TribulationType.Myriad
                or TribulationType.MinorChaos
                or TribulationType.MajorChaos => float.PositiveInfinity,
            _ => 1f
        };

        return float.IsPositiveInfinity(multiplier)
            ? 100f
            : Math.Min(100f, baseChance * multiplier);
    }

    private static float GetExtraRelicDropChance(
        TribulationType? tribulationType) => tribulationType switch
    {
        TribulationType.Earthly => 40f,
        TribulationType.Heavenly => 70f,
        TribulationType.Grand
            or TribulationType.Myriad
            or TribulationType.MinorChaos
            or TribulationType.MajorChaos => 100f,
        _ => 0f
    };

    private static int GetCombatTribulationType(Player player) =>
        player.Creature.GetPower<PlayerTribulationPower>() is { } power
            ? (int)power.CurrentType
            : NoTribulation;

    private static TribulationType? GetPayloadTribulationType(
        ShaZhaoRecipeRewardsPayload payload,
        Player player)
    {
        foreach (var entry in payload.Tribulations)
        {
            if (entry.PlayerNetId == player.NetId
                && Enum.IsDefined(typeof(TribulationType), entry.TribulationType))
            {
                return (TribulationType)entry.TribulationType;
            }
        }

        return null;
    }

    private static string GetTribulationLogSuffix(
        TribulationType? tribulationType) => tribulationType is { } type
            ? $" after {type} tribulation"
            : string.Empty;

    private static bool HasRecipeReward(CombatRoom room, Player player) =>
        room.ExtraRewards.TryGetValue(player, out var rewards)
        && rewards.OfType<RelicReward>()
            .Any(static reward => reward.Relic is AbstractRecipeRelic);

    private static List<RelicModel> GetUnownedRecipes(Player player) =>
        GetAllRecipes()
            .Where(recipe => player.Relics.All(relic => relic.Id != recipe.Id))
            .ToList();

    private static IReadOnlyList<RelicModel> GetAllRecipes() =>
    [
        ModelDb.Relic<RecipeAngryBird>(),
        ModelDb.Relic<RecipeAnQiSha>(),
        ModelDb.Relic<RecipeAnTuZhongShanBao>(),
        ModelDb.Relic<RecipeGuangYinFeiRen>(),
        ModelDb.Relic<RecipeJianHenSuoMing>(),
        ModelDb.Relic<RecipeJianLangSanDie>(),
        ModelDb.Relic<RecipeJianMianCengXiangShi>(),
        ModelDb.Relic<RecipeSanShiSanTianGuang>(),
        ModelDb.Relic<RecipeShangFangJieWa>(),
        ModelDb.Relic<RecipeSongYouFeng>(),
        ModelDb.Relic<RecipeTianPuGuangHe>(),
        ModelDb.Relic<RecipeWanWo>(),
        ModelDb.Relic<RecipeWanWoDaShouYin>(),
        ModelDb.Relic<RecipeWanWuDaTongBian>(),
        ModelDb.Relic<RecipeWanXingFeiYing>(),
        ModelDb.Relic<RecipeWeiLaiShen>(),
        ModelDb.Relic<RecipeWuJinXuanGuangQi>(),
        ModelDb.Relic<RecipeWuZhiQuanXinJian>(),
        ModelDb.Relic<RecipeXingXiuQiPan>(),
        ModelDb.Relic<RecipeYinGuoShenShu>(),
        ModelDb.Relic<RecipeXueJianLeng>(),
        ModelDb.Relic<RecipeXuePiaoLiu>(),
        ModelDb.Relic<RecipeYangMangBeiHuoYi>(),
        ModelDb.Relic<RecipeZhuiMingHuo>(),
        ModelDb.Relic<RecipeZhuMoBang>()
    ];
}
