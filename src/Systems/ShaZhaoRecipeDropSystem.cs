using GuZhenRen.Characters;
using GuZhenRen.Relics;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib;

namespace GuZhenRen.Systems;

internal static class ShaZhaoRecipeDropSystem
{
    private const float MonsterDropChance = 15f;
    private const float EliteDropChance = 50f;
    private const float BossDropChance = 100f;

    public static void TryAddCombatReward(CombatEndedEvent evt)
    {
        if (evt.Room is not CombatRoom room || !room.Encounter.ShouldGiveRewards)
        {
            return;
        }

        foreach (var player in evt.RunState.Players)
        {
            if (player.Character is not FangYuanCharacter
                || HasRecipeReward(room, player))
            {
                continue;
            }

            var candidates = GetUnownedRecipes(player);
            if (candidates.Count == 0)
            {
                Entry.Logger.Info(
                    "Sha Zhao recipe reward skipped: every recipe is already owned.");
                continue;
            }

            var chance = GetDropChance(room.RoomType);
            if (player.PlayerRng.Rewards.NextFloat(100f) >= chance)
            {
                Entry.Logger.Info(
                    $"Sha Zhao recipe reward missed: {chance:0}% chance in {room.RoomType} room.");
                continue;
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
                + $"from {room.RoomType} room at {chance:0}% chance.");
        }
    }

    private static float GetDropChance(RoomType roomType) => roomType switch
    {
        RoomType.Elite => EliteDropChance,
        RoomType.Boss => BossDropChance,
        _ => MonsterDropChance
    };

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
