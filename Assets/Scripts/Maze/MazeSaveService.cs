using System;
using System.Linq;
using ServiceLocator = Template.Services.Services;
using Template.Services.Saving;
using UnityEngine;

namespace Template.Maze
{
    public class MazeSaveService : MonoBehaviour, IMazeSaveService
    {
        [SerializeField] private string mazeFileName = "maze_save.json";
        [SerializeField] private int mazeSchemaVersion = 1;

        private MazeSaveRepository _repository;
        private MazeSaveData _saveData;

        public bool IsDirty { get; private set; }

        private void Awake()
        {
            if (ServiceLocator.MazeSaveService != null && !ReferenceEquals(ServiceLocator.MazeSaveService, this))
            {
                Destroy(gameObject);
                return;
            }

            ServiceLocator.MazeSaveService = this;

#if UNITY_WEBGL && !UNITY_EDITOR
            var storage = new WebFileSaveStorage();
#else
            var storage = new WindowsFileSaveStorage();
#endif
            _repository = new MazeSaveRepository(storage, new JsonSaveSerializer(), mazeFileName);
            _saveData = MazeSaveData.CreateEmpty(mazeSchemaVersion);

            LoadMaze();
        }

        private void OnEnable()
        {
            if (ServiceLocator.SaveService == null) return;

            ServiceLocator.SaveService.GameLoaded += LoadMaze;
            ServiceLocator.SaveService.GameSaved += SaveMaze;
            ServiceLocator.SaveService.GameDeleted += DeleteMaze;
        }

        private void OnDisable()
        {
            if (ServiceLocator.SaveService == null) return;

            ServiceLocator.SaveService.GameLoaded -= LoadMaze;
            ServiceLocator.SaveService.GameSaved -= SaveMaze;
            ServiceLocator.SaveService.GameDeleted -= DeleteMaze;
        }

        private void OnDestroy()
        {
            if (ReferenceEquals(ServiceLocator.MazeSaveService, this))
            {
                ServiceLocator.MazeSaveService = null;
            }
        }

        public MazeSessionState PrepareMazeForEntry(MazeConfig config)
        {
            if (config == null)
            {
                return CreateSessionState();
            }

            if (!_saveData.hasActiveMaze)
            {
                BeginNewMaze(config, null);
            }
            else if (_saveData.isCompleted)
            {
                BeginNewMaze(config, _saveData.activeSeed);
            }

            return CreateSessionState();
        }

        public bool IsCoinCollected(string coinId)
        {
            if (string.IsNullOrWhiteSpace(coinId) || _saveData.collectedCoinIds == null)
            {
                return false;
            }

            return _saveData.collectedCoinIds.Contains(coinId);
        }

        public void MarkCoinCollected(string coinId)
        {
            if (string.IsNullOrWhiteSpace(coinId)) return;
            if (!_saveData.hasActiveMaze) return;

            if (_saveData.collectedCoinIds == null)
            {
                _saveData.collectedCoinIds = new System.Collections.Generic.List<string>();
            }

            if (_saveData.collectedCoinIds.Contains(coinId)) return;

            _saveData.collectedCoinIds.Add(coinId);
            IsDirty = true;
        }

        public bool TryMarkMazeCompleted()
        {
            if (!_saveData.hasActiveMaze) return false;
            if (_saveData.isCompleted) return false;

            _saveData.isCompleted = true;
            IsDirty = true;
            return true;
        }

        public void SaveMaze()
        {
            if (_repository == null) return;
            if (!IsDirty) return;

            _repository.Save(_saveData, mazeSchemaVersion);
            IsDirty = false;
        }

        public void LoadMaze()
        {
            if (_repository == null) return;

            _saveData = _repository.Load(mazeSchemaVersion);
            IsDirty = false;
        }

        public void DeleteMaze()
        {
            if (_repository == null) return;

            _repository.Delete();
            _saveData = MazeSaveData.CreateEmpty(mazeSchemaVersion);
            IsDirty = false;
        }

        private void BeginNewMaze(MazeConfig config, int? previousSeed)
        {
            _saveData.hasActiveMaze = true;
            _saveData.activeSeed = ResolveSeed(config, previousSeed);
            _saveData.isCompleted = false;

            if (_saveData.collectedCoinIds == null)
            {
                _saveData.collectedCoinIds = new System.Collections.Generic.List<string>();
            }
            else
            {
                _saveData.collectedCoinIds.Clear();
            }

            IsDirty = true;
        }

        private static int ResolveSeed(MazeConfig config, int? previousSeed)
        {
            if (config.UseSeedOverride)
            {
                return MazeSeedUtility.Derive(config.SeedOverride, 0);
            }

            if (previousSeed.HasValue)
            {
                return MazeSeedUtility.GetNextSeed(previousSeed.Value);
            }

            return MazeSeedUtility.CreateRandomSeed();
        }

        private MazeSessionState CreateSessionState()
        {
            if (!_saveData.hasActiveMaze)
            {
                return new MazeSessionState(0, false, Array.Empty<string>());
            }

            var collectedIds = _saveData.collectedCoinIds != null
                ? _saveData.collectedCoinIds.ToArray()
                : Array.Empty<string>();

            return new MazeSessionState(_saveData.activeSeed, _saveData.isCompleted, collectedIds);
        }
    }
}
