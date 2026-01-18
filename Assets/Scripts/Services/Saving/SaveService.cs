using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public interface ISaveService
{
    GameDataCache GameDataCache { get; }

    bool IsGameDirty { get; }

    void MarkGameDirty();

    void Register(ILevelFlaggable levelFlaggable);
    void SetGameDataCacheFlag(string name, string id, bool value);

    void LoadGame();
    bool LoadGameExists();
    void SaveGame();
    void DeleteGame();
}

public class SaveService : MonoBehaviour, ISaveService
{
    private ISaveStorage _storage;
    private ISaveSerializer _serializer;

    [SerializeField] private string gameFileName = "game.json";
    [SerializeField] private int gameSchemaVersion = 1;

    public GameDataCache GameDataCache { get; private set; } = new GameDataCache();
    public bool IsGameDirty { get; private set; }

    // Self-registered flaggables, grouped by scene name.
    private readonly Dictionary<string, HashSet<ILevelFlaggable>> _levelFlaggablesByScene
        = new Dictionary<string, HashSet<ILevelFlaggable>>();

    private void Awake()
    {
        if (Services.SaveService != null) return;

        Services.SaveService = this;

        _storage = new FileSaveStorage();
        _serializer = new JsonSaveSerializer();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void MarkGameDirty() => IsGameDirty = true;
    public bool LoadGameExists() => _storage.Exists(gameFileName);

    public void LoadGame()
    {
        if (!_storage.Exists(gameFileName))
        {
            GameDataCache = new GameDataCache();
            IsGameDirty = false;
            return;
        }

        try
        {
            var bytes = _storage.ReadAllBytes(gameFileName);
            var dto = _serializer.Deserialize<GameFileDto>(bytes);

            if (dto == null || dto.schemaVersion != gameSchemaVersion)
            {
                GameDataCache = new GameDataCache();
                IsGameDirty = true;
                return;
            }

            var cache = new GameDataCache();
            cache.Player = dto.player ?? new PlayerSaveData();

            cache.LevelInteractableFlags.Clear();
            if (dto.scenes != null)
            {
                foreach (var sceneDto in dto.scenes)
                {
                    if (sceneDto == null || string.IsNullOrWhiteSpace(sceneDto.sceneName)) continue;

                    var map = new Dictionary<string, bool>();
                    if (sceneDto.flags != null)
                    {
                        foreach (var f in sceneDto.flags)
                        {
                            if (f == null || string.IsNullOrWhiteSpace(f.id)) continue;
                            map[f.id] = f.value;
                        }
                    }

                    cache.LevelInteractableFlags[sceneDto.sceneName] = map;
                }
            }

            GameDataCache = cache;
            IsGameDirty = false;

            // Apply for currently active scene(s)
            ApplyFlagsForScene(SceneManager.GetActiveScene().name);
        }
        catch
        {
            GameDataCache = new GameDataCache();
            IsGameDirty = true;
        }
    }

    public void SaveGame()
    {
        // Capture current scene flags before writing
        CaptureFlagsForScene(SceneManager.GetActiveScene().name);

        if (!IsGameDirty) return;

        var dto = new GameFileDto
        {
            schemaVersion = gameSchemaVersion,
            player = GameDataCache?.Player ?? new PlayerSaveData(),
            scenes = new List<SceneFlagsDto>()
        };

        if (GameDataCache != null)
        {
            foreach (var kvp in GameDataCache.LevelInteractableFlags)
            {
                var sceneName = kvp.Key;
                var flags = kvp.Value;

                var sceneDto = new SceneFlagsDto { sceneName = sceneName };
                foreach (var flagKvp in flags)
                {
                    sceneDto.flags.Add(new FlagEntryDto { id = flagKvp.Key, value = flagKvp.Value });
                }
                dto.scenes.Add(sceneDto);
            }
        }

        var bytes = _serializer.Serialize(dto);
        _storage.WriteAllBytes(gameFileName, bytes);

        IsGameDirty = false;
    }

    public void DeleteGame()
    {
        _storage.Delete(gameFileName);

        GameDataCache = new GameDataCache();

        IsGameDirty = false;
    }

    /// <summary>Flaggables call this on enable.</summary>
    public void Register(ILevelFlaggable flaggable)
    {
        if (flaggable == null) return;

        var id = flaggable.SaveId;
        if (string.IsNullOrWhiteSpace(id)) return;

        var sceneName = GetFlaggableSceneName(flaggable);
        if (string.IsNullOrWhiteSpace(sceneName)) return;

        if (!_levelFlaggablesByScene.TryGetValue(sceneName, out var set))
        {
            set = new HashSet<ILevelFlaggable>();
            _levelFlaggablesByScene[sceneName] = set;
        }

        set.Add(flaggable);

        // If we already have a saved flag for this entity, apply immediately.
        if (TryGetFlag(sceneName, id, out var value)) flaggable.ApplyFlag(value);
    }

    public void SetGameDataCacheFlag(string sceneName, string id, bool value)
    {
        if (GameDataCache == null) return;

        if (!GameDataCache.LevelInteractableFlags.TryGetValue(sceneName, out var map))
        {
            map = new Dictionary<string, bool>();
            GameDataCache.LevelInteractableFlags[sceneName] = map;
        }

        map[id] = value;
        IsGameDirty = true;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Apply flags when a scene loads (flaggables may also apply on Register).
        ApplyFlagsForScene(scene.name);
    }

    private void ApplyFlagsForScene(string sceneName)
    {
        if (GameDataCache == null) return;
        if (!_levelFlaggablesByScene.TryGetValue(sceneName, out var flaggables)) return;

        foreach (var p in flaggables)
        {
            if (p == null) continue;

            var id = p.SaveId;
            if (string.IsNullOrWhiteSpace(id)) continue;

            if (TryGetFlag(sceneName, id, out var value)) p.ApplyFlag(value);
        }
    }

    private void CaptureFlagsForScene(string sceneName)
    {
        if (GameDataCache == null) return;
        if (!_levelFlaggablesByScene.TryGetValue(sceneName, out var flaggables)) return;

        foreach (var p in flaggables)
        {
            if (p == null) continue;

            var id = p.SaveId;
            if (string.IsNullOrWhiteSpace(id)) continue;

            var value = p.GetFlag();
            SetGameDataCacheFlag(sceneName, id, value);
        }
    }

    private bool TryGetFlag(string sceneName, string id, out bool value)
    {
        value = false;
        if (GameDataCache == null) return false;

        if (!GameDataCache.LevelInteractableFlags.TryGetValue(sceneName, out var map)) return false;

        return map.TryGetValue(id, out value);
    }

    private static string GetFlaggableSceneName(ILevelFlaggable flaggable)
    {
        if (flaggable is MonoBehaviour mb) return mb.gameObject.scene.name;

        return SceneManager.GetActiveScene().name;
    }
}
