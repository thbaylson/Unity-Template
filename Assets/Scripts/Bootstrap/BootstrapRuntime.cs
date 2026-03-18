using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Template.Bootstrap
{
    public static class BootstrapRuntime
    {
        private const string ConfigResourcePath = "Config";
        // TODO: Find a way to append environment suffixes to this
        private const string ConfigResourceFile = "BootstrapConfig";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureBootstrapLoaded()
        {
            var path = Path.Combine(ConfigResourcePath, ConfigResourceFile);
            var config = Resources.Load<BootstrapConfig>(path);
            if (config == null)
            {
                Debug.LogError($"BootstrapRuntime: Could not find config in Resource subfolder \"{path}\"");
                return;
            }

            var bootstrapName = config.bootstrapSceneName;
            if (string.IsNullOrEmpty(bootstrapName))
            {
                Debug.LogError($"BootstrapRuntime: scene name \"{bootstrapName}\" could not be found.");
                return;
            }

            var activeScene = SceneManager.GetActiveScene();

            // If we already started in the Bootstrap scene, no need to load it again
            if (activeScene.name == bootstrapName) return;

            // If it's already loaded (e.g. from a previous reload), also bail
            var bootstrapScene = SceneManager.GetSceneByName(bootstrapName);
            if (bootstrapScene.isLoaded) return;

            // Load additively so the active scene stays active for now
            SceneManager.LoadScene(bootstrapName, LoadSceneMode.Additive);
        }
    }
}
