using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrapper : MonoBehaviour
{
    public static Bootstrapper Instance { get; private set; }

    [SerializeField] private BootstrapConfig config;

    // To prevent duplicate manager instantiation
    private readonly Dictionary<string, GameObject> _persistentInstances = new Dictionary<string, GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (config == null)
        {
            Debug.LogError("Bootstrapper has no BootstrapConfig assigned.", this);
            return;
        }

        InitializePersistentManagers();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void InitializePersistentManagers()
    {
        if (config.persistentManagers == null) return;

        foreach (var entry in config.persistentManagers)
        {
            if (entry == null || entry.prefab == null || string.IsNullOrEmpty(entry.key))
                continue;

            if (_persistentInstances.ContainsKey(entry.key))
                continue;

            var instance = Instantiate(entry.prefab, transform);
            instance.name = entry.prefab.name;
            _persistentInstances.Add(entry.key, instance);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Ignore the bootstrap scene itself
        if (scene.name == config.bootstrapSceneName) return;

        var profile = config.GetProfileForScene(scene.name);
        if (profile == null || profile.perSceneManagers == null) return;

        foreach (var prefab in profile.perSceneManagers)
        {
            if (prefab == null) continue;

            var instance = Instantiate(prefab);
            instance.name = prefab.name;
            SceneManager.MoveGameObjectToScene(instance, scene);
        }
    }
}
