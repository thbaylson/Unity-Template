using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Tracks scene visits in player progression data and publishes scene-related achievement
/// signals whenever a scene is loaded, enabling scene-based achievements to react through
/// the shared signal pipeline.
/// </summary>
public class SceneVisitAchievementSignalReporter : MonoBehaviour
{
    private void Start()
    {
        RegisterVisitedScene(SceneManager.GetActiveScene());
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RegisterVisitedScene(scene);
    }

    private static void RegisterVisitedScene(Scene scene)
    {
        if (!scene.IsValid() || string.IsNullOrWhiteSpace(scene.name)) return;

        var playerData = Services.SaveService?.GameDataCache?.Player;
        if (playerData == null) return;

        if (playerData.VisitedSceneNames == null)
        {
            playerData.VisitedSceneNames = new List<string>();
        }

        if (!playerData.VisitedSceneNames.Contains(scene.name))
        {
            playerData.VisitedSceneNames.Add(scene.name);
            Services.SaveService?.MarkGameDirty();
        }

        AchievementSignalBus.Publish(AchievementSignalKeys.SceneVisited);
    }
}