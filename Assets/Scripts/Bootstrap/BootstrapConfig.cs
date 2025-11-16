using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/BootstrapConfig", fileName = "BootstrapConfig")]
public class BootstrapConfig : ScriptableObject
{
    [Header("Scene Names")]
    public string bootstrapSceneName = "Bootstrap";

    [Header("Persistent Manager Prefabs")]
    [Tooltip("These managers will exist in every scene and persist across scene loads.")]
    public ManagerPrefabEntry[] persistentManagers;

    [Header("Per-Scene Manager Profiles")]
    [Tooltip("These profiles define which managers should be instantiated for specific scenes.")]
    public SceneProfile[] sceneProfiles;

    public SceneProfile GetProfileForScene(string sceneName)
    {
        foreach (var profile in sceneProfiles)
        {
            if (profile != null && profile.sceneName == sceneName)
                return profile;
        }
        return null;
    }
}

[Serializable]
public class ManagerPrefabEntry
{
    public string key;
    public GameObject prefab;
}

[Serializable]
public class SceneProfile
{
    public string sceneName;
    public GameObject[] perSceneManagers;
}
