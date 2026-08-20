using System.Reflection;
using Godot;
using HarmonyLib;
using GuZhenRen.Acts;
using GuZhenRen.Encounters;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes.Audio;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace GuZhenRen.Patches;

[HarmonyPatch]
public static class Act4Patch
{
    private static readonly PropertyInfo? RunStateProperty =
        AccessTools.Property(typeof(RunManager), "State");
    private static readonly PropertyInfo? GridProperty =
        AccessTools.Property(typeof(StandardActMap), "Grid");
    private static readonly FieldInfo? MapPointDictionaryField =
        AccessTools.Field(typeof(NMapScreen), "_mapPointDictionary");
    private static readonly FieldInfo? PathsField =
        AccessTools.Field(typeof(NMapScreen), "_paths");
    private static readonly FieldInfo? PathsContainerField =
        AccessTools.Field(typeof(NMapScreen), "_pathsContainer");
    private static readonly FieldInfo? DistYField =
        AccessTools.Field(typeof(NMapScreen), "_distY");
    private static readonly MethodInfo? DrawPathsMethod =
        AccessTools.Method(typeof(NMapScreen), "DrawPaths");

    private const string MapTopBgPath =
        "res://images/packed/map/map_bgs/glory/map_top_glory.png";
    private const string MapMidBgPath =
        "res://images/packed/map/map_bgs/glory/map_middle_glory.png";
    private const string MapBotBgPath =
        "res://images/packed/map/map_bgs/glory/map_bottom_glory.png";
    private const string RestSiteBackgroundPath =
        "res://scenes/rest_site/glory_rest_site.tscn";
    private const string BackgroundScenePath =
        "res://scenes/backgrounds/glory/glory_background.tscn";

    [HarmonyPatch(typeof(RunManager), nameof(RunManager.SetActInternal))]
    [HarmonyPostfix]
    public static void PostfixSetActInternal(RunManager __instance, int actIndex)
    {
        if (actIndex != 3)
        {
            return;
        }

        var syncField = AccessTools.GetDeclaredFields(typeof(RunManager))
            .FirstOrDefault(field => field.FieldType == typeof(MapSelectionSynchronizer));

        if (syncField?.GetValue(__instance) is not MapSelectionSynchronizer synchronizer)
        {
            return;
        }

        AccessTools.Method(typeof(MapSelectionSynchronizer), "BeforeMapGenerated")
            ?.Invoke(synchronizer, null);
        Entry.Logger.Info("Refreshed map selection synchronizer for final act.");
    }

    [HarmonyPatch(typeof(StandardActMap), "AssignPointTypes")]
    [HarmonyPrefix]
    public static bool PrefixAssignPointTypes(StandardActMap __instance)
    {
        var state = GetRunState(RunManager.Instance);
        if (state?.Act is not GuZhenRenFinalAct)
        {
            return true;
        }

        if (GridProperty?.GetValue(__instance) is not MapPoint?[,] grid)
        {
            return true;
        }

        for (var row = 1; row < grid.GetLength(1); row++)
        {
            for (var col = 0; col < grid.GetLength(0); col++)
            {
                grid[col, row] = null;
            }
        }

        SetPoint(grid, 3, 1, MapPointType.Shop);

        __instance.StartingMapPoint.PointType = MapPointType.RestSite;
        __instance.BossMapPoint.PointType = MapPointType.Boss;

        __instance.startMapPoints.Clear();
        __instance.startMapPoints.Add(grid[3, 1]!);
        __instance.StartingMapPoint.Children.Clear();
        __instance.StartingMapPoint.AddChildPoint(grid[3, 1]!);
        grid[3, 1]!.Children.Clear();
        grid[3, 1]!.AddChildPoint(__instance.BossMapPoint);
        return false;
    }

    [HarmonyPatch(typeof(ActModel), nameof(ActModel.GetNumberOfRooms))]
    [HarmonyPostfix]
    public static void PostfixGetNumberOfRooms(ActModel __instance, ref int __result)
    {
        if (__instance is GuZhenRenFinalAct)
        {
            __result = 1;
        }
    }

    [HarmonyPatch(typeof(NMapScreen), nameof(NMapScreen.SetMap))]
    [HarmonyPostfix]
    public static void PostfixSetMap(NMapScreen __instance, ActMap map)
    {
        var state = GetRunState(RunManager.Instance);
        if (state?.Act is not GuZhenRenFinalAct)
        {
            return;
        }

        if (MapPointDictionaryField?.GetValue(__instance) is not
                Dictionary<MapCoord, NMapPoint> nodes ||
            PathsField?.GetValue(__instance) is not
                Dictionary<(MapCoord, MapCoord), IReadOnlyList<TextureRect>> paths ||
            PathsContainerField?.GetValue(__instance) is not Control pathsContainer)
        {
            return;
        }

        const float startY = 800f;
        const float roomSpacing = 550f;
        DistYField?.SetValue(__instance, roomSpacing);

        foreach (var point in map.GetAllMapPoints()
                     .Append(map.StartingMapPoint)
                     .Append(map.BossMapPoint))
        {
            if (!nodes.TryGetValue(point.coord, out var node))
            {
                continue;
            }

            node.Position = new Vector2(
                node.Position.X,
                startY - point.coord.row * roomSpacing);
        }

        foreach (var child in pathsContainer.GetChildren())
        {
            child.QueueFree();
        }

        paths.Clear();
        foreach (var point in map.GetAllMapPoints())
        {
            DrawPathsMethod?.Invoke(__instance, [nodes[point.coord], point]);
        }

        DrawPathsMethod?.Invoke(
            __instance,
            [nodes[map.StartingMapPoint.coord], map.StartingMapPoint]);
        DrawPathsMethod?.Invoke(
            __instance,
            [nodes[map.BossMapPoint.coord], map.BossMapPoint]);
    }

    [HarmonyPatch(typeof(ActModel), nameof(ActModel.GenerateRooms))]
    [HarmonyPostfix]
    public static void PostfixGenerateRooms(ActModel __instance)
    {
        if (__instance is not GuZhenRenFinalAct)
        {
            return;
        }

        var rooms = AccessTools.Field(typeof(ActModel), "_rooms")
            .GetValue(__instance) as RoomSet;
        if (rooms is null)
        {
            return;
        }

        rooms.Boss = ModelDb.Encounter<LongGongEncounter>();
        rooms.eliteEncounters.Clear();
        rooms.eliteEncounters.Add(ModelDb.Encounter<ByrdonisElite>());
    }

    [HarmonyPatch(typeof(ActModel), "get_MapTopBg")]
    [HarmonyPrefix]
    public static bool PrefixMapTopBg(ActModel __instance, ref Texture2D __result)
    {
        if (__instance is not GuZhenRenFinalAct)
        {
            return true;
        }

        __result = MegaCrit.Sts2.Core.Assets.PreloadManager.Cache
            .GetCompressedTexture2D(MapTopBgPath);
        return false;
    }

    [HarmonyPatch(typeof(ActModel), "get_MapMidBg")]
    [HarmonyPrefix]
    public static bool PrefixMapMidBg(ActModel __instance, ref Texture2D __result)
    {
        if (__instance is not GuZhenRenFinalAct)
        {
            return true;
        }

        __result = MegaCrit.Sts2.Core.Assets.PreloadManager.Cache
            .GetCompressedTexture2D(MapMidBgPath);
        return false;
    }

    [HarmonyPatch(typeof(ActModel), "get_MapBotBg")]
    [HarmonyPrefix]
    public static bool PrefixMapBotBg(ActModel __instance, ref Texture2D __result)
    {
        if (__instance is not GuZhenRenFinalAct)
        {
            return true;
        }

        __result = MegaCrit.Sts2.Core.Assets.PreloadManager.Cache
            .GetCompressedTexture2D(MapBotBgPath);
        return false;
    }

    [HarmonyPatch(typeof(ActModel), "get_RestSiteBackgroundPath")]
    [HarmonyPrefix]
    public static bool PrefixRestSiteBackgroundPath(
        ActModel __instance,
        ref string __result)
    {
        if (__instance is not GuZhenRenFinalAct)
        {
            return true;
        }

        __result = RestSiteBackgroundPath;
        return false;
    }

    [HarmonyPatch(typeof(ActModel), "get_BackgroundScenePath")]
    [HarmonyPrefix]
    public static bool PrefixBackgroundScenePath(
        ActModel __instance,
        ref string __result)
    {
        if (__instance is not GuZhenRenFinalAct)
        {
            return true;
        }

        __result = BackgroundScenePath;
        return false;
    }

    [HarmonyPatch(typeof(ActModel), nameof(ActModel.GenerateBackgroundAssets))]
    [HarmonyPrefix]
    public static bool PrefixGenerateBackgroundAssets(
        ActModel __instance,
        Rng rng,
        ref BackgroundAssets __result)
    {
        if (__instance is not GuZhenRenFinalAct)
        {
            return true;
        }

        __result = new BackgroundAssets("glory", rng);
        return false;
    }

    [HarmonyPatch(typeof(NRunMusicController), "UpdateMusic")]
    [HarmonyPrefix]
    public static bool PrefixUpdateMusic()
    {
        var state = GetRunState(RunManager.Instance);
        return state?.Act is not GuZhenRenFinalAct;
    }

    [HarmonyPatch(typeof(RoomSet), nameof(RoomSet.FromSave))]
    [HarmonyPrefix]
    public static void PrefixRoomSetFromSave(SerializableRoomSet save)
    {
        save.NormalEncounterIds ??= new List<ModelId>();
        save.EliteEncounterIds ??= new List<ModelId>();
        save.EventIds ??= new List<ModelId>();

        save.NormalEncounterIds = save.NormalEncounterIds
            .Where(id => ModelDb.GetByIdOrNull<EncounterModel>(id) != null)
            .ToList();
        save.EliteEncounterIds = save.EliteEncounterIds
            .Where(id => ModelDb.GetByIdOrNull<EncounterModel>(id) != null)
            .ToList();
        save.EventIds = save.EventIds
            .Where(id => ModelDb.GetByIdOrNull<EventModel>(id) != null)
            .ToList();
    }

    private static RunState? GetRunState(RunManager? runManager) =>
        runManager is null
            ? null
            : RunStateProperty?.GetValue(runManager) as RunState;

    private static void SetPoint(
        MapPoint?[,] grid,
        int col,
        int row,
        MapPointType type)
    {
        var point = new MapPoint(col, row)
        {
            PointType = type,
            CanBeModified = false
        };
        grid[col, row] = point;
    }
}
