using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public interface IAchievementService
{
    event Action<AchievementDefinition> AchievementUnlocked;
    event Action AchievementsChanged;

    IReadOnlyList<AchievementDefinition> GetAllDefinitions();
    AchievementProgressState GetProgress(string achievementId);
}

/// <summary>
/// Tracks achievement progress and persists data in player save data.
/// This service doesn't know about specific gameplay actions like gold collection.
/// It listens to generic progression signals and evaluates only the affected achievements.
/// </summary>
public class AchievementService : MonoBehaviour, IAchievementService
{
    private const string DefinitionResourcesPath = "Achievements/";

    public event Action<AchievementDefinition> AchievementUnlocked;
    public event Action AchievementsChanged;

    private readonly Dictionary<string, AchievementDefinition> _definitionById =
        new Dictionary<string, AchievementDefinition>(StringComparer.Ordinal);

    private readonly Dictionary<string, AchievementProgressState> _progressById =
        new Dictionary<string, AchievementProgressState>(StringComparer.Ordinal);

    private readonly Dictionary<string, List<AchievementDefinition>> _definitionsBySignalKey =
        new Dictionary<string, List<AchievementDefinition>>(StringComparer.Ordinal);

    private void Awake()
    {
        if (Services.AchievementService != null)
        {
            Destroy(gameObject);
            return;
        }

        Services.AchievementService = this;

        LoadDefinitions();
        BuildSignalIndex();
    }

    private void OnEnable()
    {
        AchievementSignalBus.SignalRaised += OnSignalRaised;

        if (Services.SaveService != null)
        {
            Services.SaveService.GameLoaded += TryInitializeFromSaveData;
            Services.SaveService.GameDeleted += TryInitializeFromSaveData;
        }
    }

    private void OnDisable()
    {
        AchievementSignalBus.SignalRaised -= OnSignalRaised;

        if (Services.SaveService != null)
        {
            Services.SaveService.GameLoaded -= TryInitializeFromSaveData;
            Services.SaveService.GameDeleted -= TryInitializeFromSaveData;
        }
    }

    public IReadOnlyList<AchievementDefinition> GetAllDefinitions()
    {
        return _definitionById.Values.OrderBy(definition => definition.DisplayOrder).ToList();
    }

    public AchievementProgressState GetProgress(string achievementId)
    {
        if (string.IsNullOrWhiteSpace(achievementId)) return null;

        _progressById.TryGetValue(achievementId, out var progressState);
        return progressState;
    }

    private void TryInitializeFromSaveData()
    {
        var playerData = Services.SaveService?.GameDataCache?.Player;
        if (playerData == null) return;

        BuildProgressFromPlayerData(playerData);

        // One full scan at initialization/reset is fine and lets newly added
        // achievements backfill from existing save data.
        EvaluateAllFromCurrentState();

        // Always notify once so UI can refresh its initial state.
        AchievementsChanged?.Invoke();
    }

    private void LoadDefinitions()
    {
        _definitionById.Clear();

        var definitions = Resources.LoadAll<AchievementDefinition>(DefinitionResourcesPath);
        foreach (var definition in definitions)
        {
            if (definition == null) continue;

            if (string.IsNullOrWhiteSpace(definition.Id))
            {
                Debug.LogWarning($"Achievement definition '{definition.name}' has an empty id.");
                continue;
            }

            if (definition.UnlockCondition == null)
            {
                Debug.LogWarning($"Achievement definition '{definition.name}' is missing an unlock condition.");
                continue;
            }

            if (_definitionById.ContainsKey(definition.Id))
            {
                Debug.LogWarning($"Duplicate achievement id '{definition.Id}' detected. Keeping the first definition.");
                continue;
            }

            _definitionById.Add(definition.Id, definition);
        }
    }

    private void BuildSignalIndex()
    {
        _definitionsBySignalKey.Clear();

        foreach (var definition in _definitionById.Values)
        {
            var condition = definition.UnlockCondition;
            var signalKeys = condition.RelevantSignalKeys;

            if (signalKeys == null || signalKeys.Count == 0)
            {
                Debug.LogWarning(
                    $"Achievement definition '{definition.name}' has no RelevantSignalKeys. " +
                    "It will only be evaluated during initialization.");
                continue;
            }

            foreach (var signalKey in signalKeys)
            {
                if (string.IsNullOrWhiteSpace(signalKey)) continue;

                if (!_definitionsBySignalKey.TryGetValue(signalKey, out var definitions))
                {
                    definitions = new List<AchievementDefinition>();
                    _definitionsBySignalKey.Add(signalKey, definitions);
                }

                if (!definitions.Contains(definition))
                {
                    definitions.Add(definition);
                }
            }
        }
    }

    private void BuildProgressFromPlayerData(PlayerSaveData playerSaveData)
    {
        _progressById.Clear();

        if (playerSaveData.Achievements == null)
        {
            playerSaveData.Achievements = new List<AchievementProgressState>();
        }

        if (playerSaveData.VisitedSceneNames == null)
        {
            playerSaveData.VisitedSceneNames = new List<string>();
        }

        foreach (var savedProgressState in playerSaveData.Achievements)
        {
            if (savedProgressState == null || string.IsNullOrWhiteSpace(savedProgressState.AchievementId)) continue;

            _progressById[savedProgressState.AchievementId] = savedProgressState;
        }

        foreach (var definition in _definitionById.Values)
        {
            if (_progressById.ContainsKey(definition.Id)) continue;

            var progressState = new AchievementProgressState
            {
                AchievementId = definition.Id
            };

            _progressById.Add(definition.Id, progressState);
            playerSaveData.Achievements.Add(progressState);
        }
    }

    private void OnSignalRaised(string signalKey)
    {
        if (string.IsNullOrWhiteSpace(signalKey)) return;

        if (!_definitionsBySignalKey.TryGetValue(signalKey, out var definitions) || definitions.Count == 0) return;

        if (!TryBuildEvaluationContext(out var evaluationContext)) return;

        EvaluateDefinitions(definitions, evaluationContext);
    }

    private void EvaluateAllFromCurrentState()
    {
        if (!TryBuildEvaluationContext(out var evaluationContext)) return;

        EvaluateDefinitions(_definitionById.Values, evaluationContext);
    }

    private bool TryBuildEvaluationContext(out AchievementEvaluationContext evaluationContext)
    {
        var playerData = Services.SaveService?.GameDataCache?.Player;
        if (playerData == null)
        {
            evaluationContext = default;
            return false;
        }

        if (playerData.VisitedSceneNames == null)
        {
            playerData.VisitedSceneNames = new List<string>();
        }

        evaluationContext = new AchievementEvaluationContext(
            playerData.GoldAmount,
            playerData.TotalGoldCollected,
            playerData.VisitedSceneNames,
            SceneManager.GetActiveScene().name);

        return true;
    }

    private void EvaluateDefinitions(
        IEnumerable<AchievementDefinition> definitions,
        AchievementEvaluationContext evaluationContext)
    {
        var hasAnyUnlocks = false;
        var hasAnyProgressUpdates = false;

        foreach (var definition in definitions)
        {
            if (definition == null) continue;

            if (!_progressById.TryGetValue(definition.Id, out var progressState)) continue;

            if (progressState.IsUnlocked) continue;

            var evaluationResult = definition.UnlockCondition.Evaluate(evaluationContext, progressState);

            if (progressState.CurrentProgressValue != evaluationResult.ProgressValue)
            {
                progressState.CurrentProgressValue = evaluationResult.ProgressValue;
                hasAnyProgressUpdates = true;
            }

            if (!evaluationResult.IsUnlocked) continue;

            Unlock(definition, progressState, evaluationResult.UnlockProgressValue);
            hasAnyUnlocks = true;
        }

        if (!hasAnyUnlocks && !hasAnyProgressUpdates) return;

        if (hasAnyProgressUpdates)
        {
            Services.SaveService?.MarkGameDirty();
        }

        AchievementsChanged?.Invoke();
    }

    private void Unlock(
        AchievementDefinition definition,
        AchievementProgressState progressState,
        int unlockProgressValue)
    {
        progressState.IsUnlocked = true;
        progressState.CurrentProgressValue = unlockProgressValue;
        progressState.UnlockedUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        Services.SaveService?.MarkGameDirty();
        AchievementUnlocked?.Invoke(definition);
        Debug.Log($"Achievement unlocked: {definition.DisplayName}");
    }
}