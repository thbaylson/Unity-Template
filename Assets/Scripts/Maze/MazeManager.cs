using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Template.Achievements;
using Template.Maze.Generation;
using ServiceLocator = Template.Services.Services;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace Template.Maze
{
    public class MazeManager : MonoBehaviour
    {
        [SerializeField] private MazeConfig config;
        [SerializeField] private string returnSceneName = "FlatScene";

        private IMazeGeneratorAlgorithm _generator;
        private Transform _runtimeRoot;

        private void Awake()
        {
            if (config == null)
            {
                Debug.LogError("MazeManager is missing a MazeConfig reference.", this);
                enabled = false;
                return;
            }

            _generator = MazeGeneratorFactory.Create(config.AlgorithmKind);
        }

        private void Start()
        {
            if (_generator == null)
            {
                Debug.LogError("MazeManager could not create a maze generation algorithm.", this);
                return;
            }

            BuildMaze();
        }

        private void BuildMaze()
        {
            var mazeSaveService = ServiceLocator.MazeSaveService;
            if (mazeSaveService == null)
            {
                Debug.LogError("MazeManager requires the MazeSaveService to be initialized first.", this);
                return;
            }

            var session = mazeSaveService.PrepareMazeForEntry(config);
            var layout = _generator.Generate(new MazeGenerationRequest(config.Width, config.Height, session.Seed));

            RebuildRuntimeRoot();

            var floorsRoot = CreateSectionRoot("Floors");
            var wallsRoot = CreateSectionRoot("Walls");
            var coinsRoot = CreateSectionRoot("Coins");
            var doorsRoot = CreateSectionRoot("Doors");

            BuildFloors(layout, floorsRoot);
            BuildWalls(layout, wallsRoot);
            BuildExitDoor(layout, doorsRoot);
            BuildCoins(layout, session.Seed, session.CollectedCoinIds, coinsRoot);
        }

        private void RebuildRuntimeRoot()
        {
            if (_runtimeRoot != null)
            {
                Destroy(_runtimeRoot.gameObject);
            }

            var root = new GameObject("MazeRuntime");
            root.transform.SetParent(transform, false);
            _runtimeRoot = root.transform;
        }

        private Transform CreateSectionRoot(string sectionName)
        {
            var section = new GameObject(sectionName);
            section.transform.SetParent(_runtimeRoot, false);
            return section.transform;
        }

        private void BuildFloors(MazeLayout layout, Transform parent)
        {
            var size = new Vector3(config.CellSize, config.FloorThickness, config.CellSize);

            for (var x = 0; x < layout.Width; x++)
            {
                for (var y = 0; y < layout.Height; y++)
                {
                    var cell = layout.GetCell(x, y);
                    if (!cell.HasFloor) continue;

                    var coordinate = new MazeCoordinate(x, y);
                    var center = GetCellCenter(coordinate);
                    center.y = config.MazeOrigin.y - (config.FloorThickness * 0.5f);

                    CreateCube($"Floor_{x}_{y}", size, center, config.FloorMaterial, parent);
                }
            }
        }

        private void BuildWalls(MazeLayout layout, Transform parent)
        {
            var horizontalWallSize = new Vector3(
                config.CellSize + config.WallThickness,
                config.WallHeight,
                config.WallThickness);
            var verticalWallSize = new Vector3(
                config.WallThickness,
                config.WallHeight,
                config.CellSize + config.WallThickness);

            for (var x = 0; x < layout.Width; x++)
            {
                for (var y = 0; y < layout.Height; y++)
                {
                    var coordinate = new MazeCoordinate(x, y);
                    var center = GetCellCenter(coordinate);

                    // These offsets place each wall on the shared edge between adjacent
                    // cells so the generated mesh lines up with the logical grid exactly.
                    if (layout.HasWall(coordinate, MazeWallDirection.North))
                    {
                        var wallCenter = new Vector3(
                            center.x,
                            config.MazeOrigin.y + (config.WallHeight * 0.5f),
                            center.z - (config.CellSize * 0.5f));
                        CreateCube($"NorthWall_{x}_{y}", horizontalWallSize, wallCenter, config.WallMaterial, parent);
                    }

                    if (layout.HasWall(coordinate, MazeWallDirection.West))
                    {
                        var wallCenter = new Vector3(
                            center.x - (config.CellSize * 0.5f),
                            config.MazeOrigin.y + (config.WallHeight * 0.5f),
                            center.z);
                        CreateCube($"WestWall_{x}_{y}", verticalWallSize, wallCenter, config.WallMaterial, parent);
                    }

                    if (y == layout.Height - 1 && layout.HasWall(coordinate, MazeWallDirection.South))
                    {
                        var wallCenter = new Vector3(
                            center.x,
                            config.MazeOrigin.y + (config.WallHeight * 0.5f),
                            center.z + (config.CellSize * 0.5f));
                        CreateCube($"SouthWall_{x}_{y}", horizontalWallSize, wallCenter, config.WallMaterial, parent);
                    }

                    if (x == layout.Width - 1 && layout.HasWall(coordinate, MazeWallDirection.East))
                    {
                        var wallCenter = new Vector3(
                            center.x + (config.CellSize * 0.5f),
                            config.MazeOrigin.y + (config.WallHeight * 0.5f),
                            center.z);
                        CreateCube($"EastWall_{x}_{y}", verticalWallSize, wallCenter, config.WallMaterial, parent);
                    }
                }
            }
        }

        private void BuildExitDoor(MazeLayout layout, Transform parent)
        {
            if (config.DoorwayPrefab == null)
            {
                Debug.LogWarning("MazeManager could not spawn an exit doorway because the config is missing a doorway prefab.", this);
                return;
            }

            var doorway = Instantiate(config.DoorwayPrefab, parent);
            doorway.name = "Maze Exit Doorway";
            doorway.transform.SetPositionAndRotation(
                GetExitDoorwayPosition(layout),
                GetExitDoorwayRotation(layout.ExitDirection));
            doorway.transform.localScale = config.ExitDoorwayScale;

            var transition = doorway.GetComponentInChildren<LevelTransition>(true);
            if (transition != null)
            {
                transition.ConfigureDestination(returnSceneName, config.FlatSceneReturnPosition);
                transition.BeforeTransition += HandleExitDoorTransition;
            }

            var doorwayText = doorway.GetComponentInChildren<TMP_Text>(true);
            if (doorwayText != null)
            {
                doorwayText.text = "Flat Scene";
            }
        }

        private void HandleExitDoorTransition(Collider other)
        {
            if (other == null) return;
            if (other.GetComponentInChildren<GoldCollector>() == null) return;
            if (ServiceLocator.MazeSaveService?.TryMarkMazeCompleted() != true) return;

            var playerData = ServiceLocator.SaveService?.GameDataCache?.Player;
            if (playerData != null)
            {
                playerData.TotalMazesSolved += 1;
                ServiceLocator.SaveService?.MarkGameDirty();
            }

            AchievementSignalBus.Publish(AchievementSignalKeys.MazeCompleted);
        }

        private void BuildCoins(
            MazeLayout layout,
            int seed,
            IReadOnlyCollection<string> collectedCoinIds,
            Transform parent)
        {
            if (config.CollectablePrefab == null)
            {
                Debug.LogWarning("MazeManager could not spawn coins because the config is missing a collectable prefab.", this);
                return;
            }

            var candidateCells = GetCoinCandidateCells(layout);
            if (candidateCells.Count == 0) return;

            var random = new System.Random(MazeSeedUtility.Derive(seed, 0x51A8E2D3));
            Shuffle(candidateCells, random);

            var coinCount = System.Math.Min(
                candidateCells.Count,
                random.Next(config.MinimumCoinCount, config.MaximumCoinCount + 1));
            for (var index = 0; index < coinCount; index++)
            {
                var coinId = $"coin_{index:D3}";
                if (collectedCoinIds != null && collectedCoinIds.Contains(coinId)) continue;

                var coordinate = candidateCells[index];
                var coin = Instantiate(config.CollectablePrefab, parent);
                coin.name = $"MazeCoin_{index:D3}";
                coin.transform.position = GetCoinPosition(coordinate, random);
                coin.transform.rotation = Quaternion.identity;

                var coinAmount = 1;
                var defaultCollectable = coin.GetComponent<GoldCollectable>();
                if (defaultCollectable != null)
                {
                    coinAmount = defaultCollectable.Amount;
                    Destroy(defaultCollectable);
                }

                var saveable = coin.GetComponent<Saveable>();
                if (saveable != null)
                {
                    Destroy(saveable);
                }

                var collectable = coin.GetComponent<MazeCoinCollectable>();
                if (collectable == null)
                {
                    collectable = coin.AddComponent<MazeCoinCollectable>();
                }

                collectable.Initialize(coinId, coinAmount);
            }
        }

        private List<MazeCoordinate> GetCoinCandidateCells(MazeLayout layout)
        {
            var cells = new List<MazeCoordinate>();

            for (var x = 0; x < layout.Width; x++)
            {
                for (var y = 0; y < layout.Height; y++)
                {
                    var coordinate = new MazeCoordinate(x, y);
                    if (coordinate.Equals(layout.StartCoordinate)) continue;
                    if (coordinate.Equals(layout.ExitCoordinate)) continue;

                    cells.Add(coordinate);
                }
            }

            return cells;
        }

        private Vector3 GetCoinPosition(MazeCoordinate coordinate, System.Random random)
        {
            var center = GetCellCenter(coordinate);
            var jitterRange = config.CellSize * 0.2f;

            // Small deterministic offsets keep the layout reproducible while avoiding
            // perfectly centered coin placement in every corridor.
            var jitterX = ((float)random.NextDouble() * 2f - 1f) * jitterRange;
            var jitterZ = ((float)random.NextDouble() * 2f - 1f) * jitterRange;

            return new Vector3(
                center.x + jitterX,
                config.MazeOrigin.y + config.CoinHeight,
                center.z + jitterZ);
        }

        private Vector3 GetCellCenter(MazeCoordinate coordinate)
        {
            return config.MazeOrigin + new Vector3(
                coordinate.X * config.CellSize,
                0f,
                coordinate.Y * config.CellSize);
        }

        private Vector3 GetExitDoorwayPosition(MazeLayout layout)
        {
            var exitCenter = GetCellCenter(layout.ExitCoordinate);
            var halfCell = config.CellSize * 0.5f;
            var rotation = GetExitDoorwayRotation(layout.ExitDirection);
            var basePosition = exitCenter;

            switch (layout.ExitDirection)
            {
                case MazeWallDirection.North:
                    basePosition = new Vector3(exitCenter.x, config.MazeOrigin.y + config.DoorwayCenterHeight, exitCenter.z - halfCell);
                    break;
                case MazeWallDirection.East:
                    basePosition = new Vector3(exitCenter.x + halfCell, config.MazeOrigin.y + config.DoorwayCenterHeight, exitCenter.z);
                    break;
                case MazeWallDirection.South:
                    basePosition = new Vector3(exitCenter.x, config.MazeOrigin.y + config.DoorwayCenterHeight, exitCenter.z + halfCell);
                    break;
                default:
                    basePosition = new Vector3(exitCenter.x - halfCell, config.MazeOrigin.y + config.DoorwayCenterHeight, exitCenter.z);
                    break;
            }

            return basePosition + (rotation * config.ExitDoorwayPositionOffset);
        }

        private static Quaternion GetExitDoorwayRotation(MazeWallDirection direction)
        {
            switch (direction)
            {
                case MazeWallDirection.North:
                    return Quaternion.identity;
                case MazeWallDirection.East:
                    return Quaternion.Euler(0f, -90f, 0f);
                case MazeWallDirection.South:
                    return Quaternion.Euler(0f, 180f, 0f);
                default:
                    return Quaternion.Euler(0f, 90f, 0f);
            }
        }

        private static void Shuffle<T>(IList<T> items, System.Random random)
        {
            for (var index = items.Count - 1; index > 0; index--)
            {
                var swapIndex = random.Next(index + 1);
                (items[index], items[swapIndex]) = (items[swapIndex], items[index]);
            }
        }

        private static void EnsureCollider(ProBuilderMesh mesh)
        {
            var filter = mesh.GetComponent<MeshFilter>();
            if (filter == null) return;

            var meshCollider = mesh.GetComponent<MeshCollider>();
            if (meshCollider == null)
            {
                meshCollider = mesh.gameObject.AddComponent<MeshCollider>();
            }

            meshCollider.sharedMesh = filter.sharedMesh;
        }

        private void CreateCube(
            string objectName,
            Vector3 size,
            Vector3 position,
            Material material,
            Transform parent)
        {
            var cube = ShapeGenerator.GenerateCube(PivotLocation.Center, size);
            cube.gameObject.name = objectName;
            cube.transform.SetParent(parent, false);
            cube.transform.position = position;

            var renderer = cube.GetComponent<MeshRenderer>();
            if (renderer != null && material != null)
            {
                renderer.sharedMaterial = material;
            }

            cube.ToMesh();
            cube.Refresh();
            EnsureCollider(cube);
        }
    }
}
