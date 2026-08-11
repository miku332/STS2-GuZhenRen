using System.Reflection;
using System.Text.Json.Nodes;
using GuZhenRen.CardPools;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Interactions.RightClick;
using STS2RitsuLib.RunData;
using STS2RitsuLib.Scaffolding.Content;

namespace GuZhenRen.Relics;

[RegisterRelic(typeof(GuZhenRenRelicPool))]
public sealed class ChunQiuChan : ModRelicTemplate, IModRightClickableRelic
{
    private const int MaxSnapshots = 8;
    private const int CooldownFloors = 10;
    private const int MinimumBacktrack = 4;
    private const int BaseFailureChance = 5;
    private const int FailureChancePerUse = 10;
    private const int HongYunReduction = 40;
    private const int GouShiYunReduction = 25;

    private static readonly RunSavedData<ChunQiuChanRunData> SavedData =
        RunSavedDataStore.For(Entry.ModId).Register(
            "chun-qiu-chan",
            static () => new ChunQiuChanRunData());

    private static bool _capturingSnapshot;
    private static readonly Type? RunSavedDataRegistryType =
        typeof(RunSavedDataStore).Assembly.GetType("STS2RitsuLib.RunData.RunSavedDataRegistry");

    public override RelicRarity Rarity => RelicRarity.Rare;

    public override bool ShowCounter => true;

    public override int DisplayAmount
    {
        get
        {
            if (!IsMutable || Owner is null)
            {
                return 0;
            }

            var runState = CurrentRunState;
            if (runState is null)
            {
                return 0;
            }

            var data = SavedData.Get(runState);
            return data.Cooldown > 0 ? data.Cooldown : data.History.Count;
        }
    }

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("FailChance", GetFailureChance()),
        new DynamicVar("Cooldown", GetCooldown()),
        new DynamicVar("Snapshots", GetSnapshotCount())
    ];

    public override RelicAssetProfile AssetProfile => new(
        IconPath: "res://GuZhenRen/images/relics/ChunQiuChan.png",
        IconOutlinePath: "res://GuZhenRen/images/relics/outline/ChunQiuChan.png",
        BigIconPath: "res://GuZhenRen/images/relics/ChunQiuChan.png");

    public bool CanHandleRightClickLocal(ModRightClickContext context)
    {
        if (context.Player != Owner
            || !Owner.Creature.IsAlive
            || !IsMutable
            || CombatManager.Instance.IsInProgress)
        {
            return false;
        }

        var runState = CurrentRunState;
        if (runState is null)
        {
            return false;
        }

        var data = SavedData.Get(runState);
        return data.Cooldown <= 0 && data.History.Count > 0;
    }

    public async Task OnRightClick(ModRightClickExecutionContext context)
    {
        if (!CanHandleRightClickLocal(new ModRightClickContext(
                context.Player,
                context.Model,
                context.Trigger)))
        {
            return;
        }

        var runState = CurrentRunState;
        if (runState is null)
        {
            return;
        }

        var data = SavedData.Get(runState);
        if (data.History.Count == 0)
        {
            return;
        }

        var targetIndex = Math.Max(0, data.History.Count - 1 - MinimumBacktrack);
        var targetJson = data.History[targetIndex];
        var failureChance = GetFailureChance();

        data.UseCount++;
        data.Cooldown = CooldownFloors;
        SavedData.Set(runState, data);
        RefreshPresentation();
        Flash();

        if (Owner.PlayerRng.Rewards.NextFloat(100f) < failureChance)
        {
            await CreatureCmd.Kill(Owner.Creature, force: true);
            return;
        }

        try
        {
            var currentSave = RunManager.Instance.ToSave(null);
            var currentPayload = BuildRunDataPayload(currentSave);
            var mergedTargetJson = MergeRunData(targetJson, currentPayload);
            var readResult = SaveManager.FromJson<SerializableRun>(mergedTargetJson);
            if (!readResult.Success || readResult.SaveData is null)
            {
                Entry.Logger.Warn("ChunQiuChan failed to deserialize the selected rewind point.");
                return;
            }

            AttachRunData(readResult.SaveData, mergedTargetJson);
            await SaveManager.Instance.IncrementNumReloads(readResult.SaveData, isMultiplayer: false);
            var game = NGame.Instance;
            if (game is null)
            {
                Entry.Logger.Warn("ChunQiuChan saved the rewind point, but the main menu is unavailable.");
                return;
            }

            await game.ReturnToMainMenuAfterRun();
        }
        catch (Exception ex)
        {
            Entry.Logger.Error($"ChunQiuChan rewind failed: {ex}");
        }
    }

    public override Task AfterObtained()
    {
        EnsureRunData();
        RefreshPresentation();
        CaptureSnapshot();
        return Task.CompletedTask;
    }

    public override Task AfterRoomEntered(AbstractRoom room)
    {
        if (!IsMutable || Owner is null)
        {
            return Task.CompletedTask;
        }

        var runState = CurrentRunState;
        if (runState is null)
        {
            return Task.CompletedTask;
        }

        var data = SavedData.Get(runState);
        if (data.Cooldown > 0)
        {
            data.Cooldown--;
            SavedData.Set(runState, data);
        }

        RefreshPresentation();
        CaptureSnapshot();
        return Task.CompletedTask;
    }

    private void EnsureRunData()
    {
        if (IsMutable && Owner is not null)
        {
            if (CurrentRunState is { } runState)
            {
                SavedData.Get(runState);
            }
        }
    }

    private void RefreshPresentation()
    {
        if (!IsMutable || Owner is null)
        {
            return;
        }

        DynamicVars["FailChance"].BaseValue = GetFailureChance();
        DynamicVars["Cooldown"].BaseValue = GetCooldown();
        DynamicVars["Snapshots"].BaseValue = GetSnapshotCount();
        Status = GetCooldown() > 0 ? RelicStatus.Disabled : RelicStatus.Normal;
        InvokeDisplayAmountChanged();
    }

    private int GetFailureChance()
    {
        if (!IsMutable || Owner is null)
        {
            return BaseFailureChance;
        }

        var runState = CurrentRunState;
        if (runState is null)
        {
            return BaseFailureChance;
        }

        var data = SavedData.Get(runState);
        var chance = BaseFailureChance + FailureChancePerUse * data.UseCount;
        if (Owner.GetRelic<HongYunQiTianGu>() is not null)
        {
            chance -= HongYunReduction;
        }

        if (Owner.GetRelic<GouShiYun>() is not null)
        {
            chance -= GouShiYunReduction;
        }

        return Math.Clamp(chance, 0, 100);
    }

    private int GetCooldown() =>
        !IsMutable || Owner is null || CurrentRunState is not { } runState
            ? 0
            : SavedData.Get(runState).Cooldown;

    private int GetSnapshotCount() =>
        !IsMutable || Owner is null || CurrentRunState is not { } runState
            ? 0
            : SavedData.Get(runState).History.Count;

    private void CaptureSnapshot()
    {
        if (_capturingSnapshot
            || !IsMutable
            || Owner is null
            || !RunManager.Instance.IsInProgress)
        {
            return;
        }

        _capturingSnapshot = true;
        var runState = CurrentRunState;
        if (runState is null)
        {
            _capturingSnapshot = false;
            return;
        }

        var data = SavedData.Get(runState);
        var history = data.History.ToList();
        data.History.Clear();

        try
        {
            var save = RunManager.Instance.ToSave(null);
            var snapshot = BuildSaveJson(save);
            if (!string.IsNullOrWhiteSpace(snapshot))
            {
                data.History.AddRange(history);
                data.History.Add(snapshot);
                while (data.History.Count > MaxSnapshots)
                {
                    data.History.RemoveAt(0);
                }

                SavedData.Set(runState, data);
            }
        }
        catch (Exception ex)
        {
            data.History.Clear();
            data.History.AddRange(history);
            SavedData.Set(runState, data);
            Entry.Logger.Warn($"ChunQiuChan snapshot failed: {ex.Message}");
        }
        finally
        {
            _capturingSnapshot = false;
            RefreshPresentation();
        }
    }

    private static string BuildSaveJson(SerializableRun save)
    {
        var json = SaveManager.ToJson(save);
        var payload = BuildRunDataPayload(save);
        return MergeRunData(json, payload);
    }

    private static string? BuildRunDataPayload(SerializableRun save)
    {
        var method = RunSavedDataRegistryType?.GetMethod(
            "BuildPayloadFromSerializable",
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        return method?.Invoke(null, [save]) as string;
    }

    private static void AttachRunData(SerializableRun save, string json)
    {
        var method = RunSavedDataRegistryType?.GetMethod(
            "AttachDocumentFromJson",
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        method?.Invoke(null, [save, json]);
    }

    private static string MergeRunData(string json, string? payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return json;
        }

        var root = JsonNode.Parse(json)?.AsObject()
            ?? throw new InvalidOperationException("Run save JSON root is not an object.");
        var payloadRoot = JsonNode.Parse(payload)?.AsObject();
        if (payloadRoot?["_ritsulib"] is JsonNode runData)
        {
            root["_ritsulib"] = runData.DeepClone();
        }

        return root.ToJsonString();
    }

    private RunState? CurrentRunState => Owner?.RunState as RunState;

    public sealed class ChunQiuChanRunData
    {
        public List<string> History { get; set; } = [];
        public int UseCount { get; set; }
        public int Cooldown { get; set; }
    }
}
