using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Template.Achievements;
using Template.Maze.Generation;
using ServiceLocator = Template.Services.Services;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

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
            var size = new Vector3(
                layout.Width * config.CellSize,
                config.FloorThickness,
                layout.Height * config.CellSize);
            var center = config.MazeOrigin + new Vector3(
                ((layout.Width - 1) * config.CellSize) * 0.5f,
                -(config.FloorThickness * 0.5f),
                ((layout.Height - 1) * config.CellSize) * 0.5f);

            CreateCube("Floor", size, center, config.FloorMaterial, parent);
        }

        private void BuildWalls(MazeLayout layout, Transform parent)
        {
            var wallSegments = CollectWallSegments(layout);
            CreateCombinedCubes("Walls", wallSegments, config.WallMaterial, parent);
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

        private List<BoxPrimitiveSpec> CollectWallSegments(MazeLayout layout)
        {
            var segments = new List<BoxPrimitiveSpec>();
            var wallCenterHeight = config.MazeOrigin.y + (config.WallHeight * 0.5f);
            var halfCell = config.CellSize * 0.5f;

            for (var y = 0; y < layout.Height; y++)
            {
                var x = 0;
                while (x < layout.Width)
                {
                    var coordinate = new MazeCoordinate(x, y);
                    if (!layout.HasWall(coordinate, MazeWallDirection.North))
                    {
                        x++;
                        continue;
                    }

                    var runStart = x;
                    while (x < layout.Width && layout.HasWall(new MazeCoordinate(x, y), MazeWallDirection.North))
                    {
                        x++;
                    }

                    var runLength = x - runStart;
                    var startCenter = GetCellCenter(new MazeCoordinate(runStart, y));
                    var endCenter = GetCellCenter(new MazeCoordinate(x - 1, y));
                    var position = new Vector3(
                        (startCenter.x + endCenter.x) * 0.5f,
                        wallCenterHeight,
                        startCenter.z - halfCell);

                    segments.Add(new BoxPrimitiveSpec(
                        $"NorthWallRun_{runStart}_{y}",
                        new Vector3((runLength * config.CellSize) + config.WallThickness, config.WallHeight, config.WallThickness),
                        position,
                        Quaternion.identity));
                }
            }

            if (layout.Height > 0)
            {
                var boundaryY = layout.Height - 1;
                var x = 0;
                while (x < layout.Width)
                {
                    var coordinate = new MazeCoordinate(x, boundaryY);
                    if (!layout.HasWall(coordinate, MazeWallDirection.South))
                    {
                        x++;
                        continue;
                    }

                    var runStart = x;
                    while (x < layout.Width && layout.HasWall(new MazeCoordinate(x, boundaryY), MazeWallDirection.South))
                    {
                        x++;
                    }

                    var runLength = x - runStart;
                    var startCenter = GetCellCenter(new MazeCoordinate(runStart, boundaryY));
                    var endCenter = GetCellCenter(new MazeCoordinate(x - 1, boundaryY));
                    var position = new Vector3(
                        (startCenter.x + endCenter.x) * 0.5f,
                        wallCenterHeight,
                        startCenter.z + halfCell);

                    segments.Add(new BoxPrimitiveSpec(
                        $"SouthWallRun_{runStart}_{boundaryY}",
                        new Vector3((runLength * config.CellSize) + config.WallThickness, config.WallHeight, config.WallThickness),
                        position,
                        Quaternion.identity));
                }
            }

            for (var x = 0; x < layout.Width; x++)
            {
                var y = 0;
                while (y < layout.Height)
                {
                    var coordinate = new MazeCoordinate(x, y);
                    if (!layout.HasWall(coordinate, MazeWallDirection.West))
                    {
                        y++;
                        continue;
                    }

                    var runStart = y;
                    while (y < layout.Height && layout.HasWall(new MazeCoordinate(x, y), MazeWallDirection.West))
                    {
                        y++;
                    }

                    var runLength = y - runStart;
                    var startCenter = GetCellCenter(new MazeCoordinate(x, runStart));
                    var endCenter = GetCellCenter(new MazeCoordinate(x, y - 1));
                    var position = new Vector3(
                        startCenter.x - halfCell,
                        wallCenterHeight,
                        (startCenter.z + endCenter.z) * 0.5f);

                    segments.Add(new BoxPrimitiveSpec(
                        $"WestWallRun_{x}_{runStart}",
                        new Vector3(config.WallThickness, config.WallHeight, (runLength * config.CellSize) + config.WallThickness),
                        position,
                        Quaternion.identity));
                }
            }

            if (layout.Width > 0)
            {
                var boundaryX = layout.Width - 1;
                var y = 0;
                while (y < layout.Height)
                {
                    var coordinate = new MazeCoordinate(boundaryX, y);
                    if (!layout.HasWall(coordinate, MazeWallDirection.East))
                    {
                        y++;
                        continue;
                    }

                    var runStart = y;
                    while (y < layout.Height && layout.HasWall(new MazeCoordinate(boundaryX, y), MazeWallDirection.East))
                    {
                        y++;
                    }

                    var runLength = y - runStart;
                    var startCenter = GetCellCenter(new MazeCoordinate(boundaryX, runStart));
                    var endCenter = GetCellCenter(new MazeCoordinate(boundaryX, y - 1));
                    var position = new Vector3(
                        startCenter.x + halfCell,
                        wallCenterHeight,
                        (startCenter.z + endCenter.z) * 0.5f);

                    segments.Add(new BoxPrimitiveSpec(
                        $"EastWallRun_{boundaryX}_{runStart}",
                        new Vector3(config.WallThickness, config.WallHeight, (runLength * config.CellSize) + config.WallThickness),
                        position,
                        Quaternion.identity));
                }
            }

            return segments;
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

        private void CreateCombinedCubes(
            string objectName,
            IReadOnlyList<BoxPrimitiveSpec> primitives,
            Material material,
            Transform parent)
        {
            if (primitives == null || primitives.Count == 0)
            {
                return;
            }

            var meshes = new List<ProBuilderMesh>(primitives.Count);
            for (var index = 0; index < primitives.Count; index++)
            {
                meshes.Add(CreateCubeMesh(primitives[index], material));
            }

            if (meshes.Count == 1)
            {
                FinalizeGeneratedMesh(meshes[0], objectName, material, parent);
                return;
            }

            var target = meshes[0];
            var combinedMeshes = CombineMeshes.Combine(meshes, target) ?? new List<ProBuilderMesh> { target };
            var retainedMeshes = new HashSet<ProBuilderMesh>(combinedMeshes);

            foreach (var mesh in meshes)
            {
                if (!retainedMeshes.Contains(mesh))
                {
                    Destroy(mesh.gameObject);
                }
            }

            for (var index = 0; index < combinedMeshes.Count; index++)
            {
                var name = combinedMeshes.Count == 1 ? objectName : $"{objectName}_{index:D2}";
                FinalizeGeneratedMesh(combinedMeshes[index], name, material, parent);
            }
        }

        private ProBuilderMesh CreateCubeMesh(BoxPrimitiveSpec primitive, Material material)
        {
            var cube = ShapeGenerator.GenerateCube(PivotLocation.Center, primitive.Size);
            cube.gameObject.name = primitive.Name;
            cube.transform.SetPositionAndRotation(primitive.Position, primitive.Rotation);

            var renderer = cube.GetComponent<MeshRenderer>();
            if (renderer != null && material != null)
            {
                renderer.sharedMaterial = material;
            }

            cube.ToMesh();
            cube.Refresh();
            return cube;
        }

        private static void FinalizeGeneratedMesh(
            ProBuilderMesh mesh,
            string objectName,
            Material material,
            Transform parent)
        {
            if (mesh == null)
            {
                return;
            }

            mesh.gameObject.name = objectName;
            mesh.transform.SetParent(parent, true);

            var renderer = mesh.GetComponent<MeshRenderer>();
            if (renderer != null && material != null)
            {
                renderer.sharedMaterial = material;
            }

            mesh.ToMesh();
            mesh.Refresh();
            EnsureCollider(mesh);
        }

        private void CreateCube(
            string objectName,
            Vector3 size,
            Vector3 position,
            Material material,
            Transform parent)
        {
            var cube = CreateCubeMesh(
                new BoxPrimitiveSpec(objectName, size, position, Quaternion.identity),
                material);
            FinalizeGeneratedMesh(cube, objectName, material, parent);
        }

        private readonly struct BoxPrimitiveSpec
        {
            public BoxPrimitiveSpec(string name, Vector3 size, Vector3 position, Quaternion rotation)
            {
                Name = name;
                Size = size;
                Position = position;
                Rotation = rotation;
            }

            public string Name { get; }
            public Vector3 Size { get; }
            public Vector3 Position { get; }
            public Quaternion Rotation { get; }
        }
    }
}
