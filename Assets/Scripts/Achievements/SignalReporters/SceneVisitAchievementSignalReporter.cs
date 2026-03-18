using System.Collections.Generic;
using ServiceLocator = Template.Services.Services;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Scripting.APIUpdating;

namespace Template.Achievements.SignalReporters
{
    /// <summary>
    /// Tracks scene visits in player progression data and publishes scene-related achievement
    /// signals whenever a scene is loaded, enabling scene-based achievements to react through
    /// the shared signal pipeline.
    /// </summary>
    [MovedFrom(true, null, "Assembly-CSharp", "SceneVisitAchievementSignalReporter")]
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

            var playerData = ServiceLocator.SaveService?.GameDataCache?.Player;
            if (playerData == null) return;

            if (playerData.VisitedSceneNames == null)
            {
                playerData.VisitedSceneNames = new List<string>();
            }

            if (!playerData.VisitedSceneNames.Contains(scene.name))
            {
                playerData.VisitedSceneNames.Add(scene.name);
                ServiceLocator.SaveService?.MarkGameDirty();
            }

            AchievementSignalBus.Publish(AchievementSignalKeys.SceneVisited);
        }
    }
}
