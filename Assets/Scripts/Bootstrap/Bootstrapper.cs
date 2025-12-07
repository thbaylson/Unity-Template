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

    private void Start()
    {
        // The Bootstrap scene is loaded additively at runtime AFTER
        // the scene that was run from the editor.
        ProcessLoadedScenes();
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

    private void ProcessLoadedScenes()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            TrySetupScene(scene);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TrySetupScene(scene);
    }

    private void TrySetupScene(Scene scene)
    {
        if (!scene.isLoaded) return;
        if (config == null) return;

        // Ignore bootstrap scene
        if (scene.name == config.bootstrapSceneName) return;

        var profile = config.GetProfileForScene(scene.name);
        if (profile == null || profile.perSceneManagers == null) return;

        foreach (var prefab in profile.perSceneManagers)
        {
            if (prefab == null) continue;

            var instance = Instantiate(prefab);
            instance.name = prefab.name;

            // Ensure the object belongs to the target content scene
            SceneManager.MoveGameObjectToScene(instance, scene);
        }
    }
}
