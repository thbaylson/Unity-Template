using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrapper : MonoBehaviour
{
    public static Bootstrapper Instance { get; private set; }

    [SerializeField] private BootstrapConfig config;

    // Track managers to prevent duplicate instantiations.
    private readonly Dictionary<string, GameObject> _persistentInstances = new Dictionary<string, GameObject>();
    private readonly Dictionary<string, GameObject> _sceneManagers = new Dictionary<string, GameObject>();

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
        Services.SaveService?.LoadGame();

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
            if (entry == null || entry.prefab == null || string.IsNullOrEmpty(entry.prefab.name)) continue;
            if (_persistentInstances.ContainsKey(entry.prefab.name)) continue;

            var instance = Instantiate(entry.prefab, transform);
            instance.name = entry.prefab.name;
            _persistentInstances.Add(entry.prefab.name, instance);
        }
    }

    private void ProcessLoadedScenes()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            LoadSceneManagers(scene);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!scene.isLoaded) return;
        if (config == null) return;

        // Ignore bootstrap scene.
        if (scene.name == config.bootstrapSceneName) return;

        UnloadSceneManagers(scene);
        LoadSceneManagers(scene);
    }

    // Unload any scene managers from the previous scene if they are not in the next scene.
    private void UnloadSceneManagers(Scene scene)
    {
        if(_sceneManagers.Count == 0) return;

        var profile = config.GetProfileForScene(scene.name);
        if (profile == null)
        {
            foreach (var instance in _sceneManagers)
            {
                Destroy(instance.Value);
            }
            _sceneManagers.Clear();
            return;
        }

        var nextSceneManagerNames = profile.perSceneManagers.Select(m => m.name).ToHashSet();
        var instancesToRemove = new List<string>();
        foreach (var instance in _sceneManagers)
        {
            if (!nextSceneManagerNames.Contains(instance.Key))
            {
                instancesToRemove.Add(instance.Key);
                Destroy(instance.Value);
            }
        }

        foreach (var name in instancesToRemove)
        {
            _sceneManagers.Remove(name);
        }
    }

    private void LoadSceneManagers(Scene scene)
    {
        var profile = config.GetProfileForScene(scene.name);
        if (profile == null || profile.perSceneManagers == null) return;

        foreach (var prefab in profile.perSceneManagers)
        {
            if (prefab == null) continue;
            if (_sceneManagers.ContainsKey(prefab.name)) continue;

            var instance = Instantiate(prefab);
            instance.name = prefab.name;
            _sceneManagers.Add(prefab.name, instance);
        }
    }
}
