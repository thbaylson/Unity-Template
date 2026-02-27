using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/BootstrapConfig", fileName = "BootstrapConfig")]
public class BootstrapConfig : ScriptableObject
{
    [Header("Bootstrap Scene Name")]
    public string bootstrapSceneName = "Bootstrap";

    [Header("Persistent Service Prefabs")]
    [Tooltip("These services will exist in every scene and persist across scene loads.")]
    public ServicePrefabEntry[] persistentServices;

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
public class ServicePrefabEntry
{
    public GameObject prefab;
}

[Serializable]
public class SceneProfile
{
    public string sceneName;
    public GameObject[] perSceneManagers;
}
