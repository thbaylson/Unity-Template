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

    void RegisterGoldCollected(int collectedAmount, int currentGoldOwned);
    void RegisterSceneVisited(string sceneName);
}

/// <summary>
/// Tracks achievement progress and persists data in player save data.
/// </summary>
public class AchievementService : MonoBehaviour, IAchievementService
{
    private const string DefinitionResourcesPath = "Achievements/Definitions";

    public event Action<AchievementDefinition> AchievementUnlocked;
    public event Action AchievementsChanged;

    private readonly Dictionary<string, AchievementDefinition> _definitionById = new Dictionary<string, AchievementDefinition>();
    private readonly Dictionary<string, AchievementProgressState> _progressById = new Dictionary<string, AchievementProgressState>();

    private bool _isInitialized;

    private void Awake()
    {
        if (Services.AchievementService != null && Services.AchievementService != this)
        {
            Destroy(gameObject);
            return;
        }

        Services.AchievementService = this;
        DontDestroyOnLoad(gameObject);
        LoadDefinitions();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        if (_isInitialized) return;
        TryInitializeFromSaveData();
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

    public void RegisterGoldCollected(int collectedAmount, int currentGoldOwned)
    {
        if (!_isInitialized) return;
        if (collectedAmount <= 0) return;

        var playerData = Services.SaveService?.GameDataCache?.Player;
        if (playerData == null) return;

        playerData.TotalGoldCollected += collectedAmount;
        EvaluateAllAchievements(currentGoldOwned, playerData.TotalGoldCollected, SceneManager.GetActiveScene().name, true);
    }

    public void RegisterSceneVisited(string sceneName)
    {
        if (!_isInitialized) return;
        if (string.IsNullOrWhiteSpace(sceneName)) return;

        var playerData = Services.SaveService?.GameDataCache?.Player;
        if (playerData == null) return;

        if (!playerData.VisitedSceneNames.Contains(sceneName))
        {
            playerData.VisitedSceneNames.Add(sceneName);
            Services.SaveService?.MarkGameDirty();
        }

        var currentGoldOwned = playerData.GoldAmount;
        var totalGoldCollected = playerData.TotalGoldCollected;

        EvaluateAllAchievements(currentGoldOwned, totalGoldCollected, sceneName, false);
    }

    private void TryInitializeFromSaveData()
    {
        var saveService = Services.SaveService;
        if (saveService == null) return;
        if (saveService.GameDataCache?.Player == null) return;

        BuildProgressFromPlayerData(saveService.GameDataCache.Player);
        _isInitialized = true;

        RegisterSceneVisited(SceneManager.GetActiveScene().name);
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

            if (_definitionById.ContainsKey(definition.Id))
            {
                Debug.LogWarning($"Duplicate achievement id '{definition.Id}' detected. Keeping the first definition.");
                continue;
            }

            _definitionById.Add(definition.Id, definition);
        }
    }

    private void BuildProgressFromPlayerData(PlayerSaveData playerSaveData)
    {
        _progressById.Clear();

        if (playerSaveData.Achievements == null)
        {
            playerSaveData.Achievements = new List<AchievementProgressState>();
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
                AchievementId = definition.Id,
            };
            _progressById.Add(definition.Id, progressState);
            playerSaveData.Achievements.Add(progressState);
        }
    }

    private void EvaluateAllAchievements(int currentGoldOwned, int totalGoldCollected, string mostRecentScene, bool markSaveDirty)
    {
        var hasAnyUnlocks = false;
        var hasAnyProgressUpdates = false;

        foreach (var definition in _definitionById.Values)
        {
            if (!_progressById.TryGetValue(definition.Id, out var progressState)) continue;
            if (progressState.IsUnlocked) continue;

            switch (definition.ConditionType)
            {
                case AchievementConditionType.TotalGoldOwnedAtLeast:
                    var clampedGoldOwnedProgress = Mathf.Clamp(currentGoldOwned, 0, definition.RequiredAmount);
                    if (progressState.CurrentProgressValue != clampedGoldOwnedProgress)
                    {
                        progressState.CurrentProgressValue = clampedGoldOwnedProgress;
                        hasAnyProgressUpdates = true;
                    }
                    if (currentGoldOwned >= definition.RequiredAmount)
                    {
                        Unlock(definition, progressState);
                        hasAnyUnlocks = true;
                    }
                    break;
                case AchievementConditionType.TotalGoldCollectedAtLeast:
                    var clampedTotalGoldProgress = Mathf.Clamp(totalGoldCollected, 0, definition.RequiredAmount);
                    if (progressState.CurrentProgressValue != clampedTotalGoldProgress)
                    {
                        progressState.CurrentProgressValue = clampedTotalGoldProgress;
                        hasAnyProgressUpdates = true;
                    }
                    if (totalGoldCollected >= definition.RequiredAmount)
                    {
                        Unlock(definition, progressState);
                        hasAnyUnlocks = true;
                    }
                    break;
                case AchievementConditionType.SceneVisited:
                    progressState.CurrentProgressValue = 0;
                    if (string.Equals(definition.RequiredSceneName, mostRecentScene, StringComparison.Ordinal))
                    {
                        Unlock(definition, progressState);
                        hasAnyUnlocks = true;
                    }
                    break;
            }
        }

        if (!hasAnyUnlocks && !hasAnyProgressUpdates) return;

        AchievementsChanged?.Invoke();
        if (markSaveDirty || hasAnyProgressUpdates)
        {
            Services.SaveService?.MarkGameDirty();
        }
    }

    private void Unlock(AchievementDefinition definition, AchievementProgressState progressState)
    {
        progressState.IsUnlocked = true;
        progressState.CurrentProgressValue = definition.RequiredAmount;
        progressState.UnlockedUnixTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        Services.SaveService?.MarkGameDirty();
        AchievementUnlocked?.Invoke(definition);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RegisterSceneVisited(scene.name);
    }
}
