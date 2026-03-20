using UnityEngine;

namespace Template.Maze
{
    [CreateAssetMenu(menuName = "Config/MazeConfig", fileName = "MazeConfig")]
    public class MazeConfig : ScriptableObject
    {
        [Header("Algorithm")]
        [SerializeField] private MazeAlgorithmKind algorithmKind = MazeAlgorithmKind.RecursiveBacktracker;

        [Header("Layout")]
        [SerializeField] [Min(2)] private int width = 10;
        [SerializeField] [Min(2)] private int height = 10;
        [SerializeField] [Min(2f)] private float cellSize = 6f;
        [SerializeField] [Min(0.05f)] private float floorThickness = 0.25f;
        [SerializeField] [Min(0.05f)] private float wallThickness = 0.4f;
        [SerializeField] [Min(1f)] private float wallHeight = 4f;
        [SerializeField] private Vector3 mazeOrigin = Vector3.zero;

        [Header("Seed")]
        [SerializeField] private bool useSeedOverride;
        [SerializeField] private int seedOverride = 12345;

        [Header("Collectables")]
        [SerializeField] [Min(0)] private int minimumCoinCount = 4;
        [SerializeField] [Min(0)] private int maximumCoinCount = 9;
        [SerializeField] [Min(0f)] private float coinHeight = 1f;
        [SerializeField] private GameObject collectablePrefab;

        [Header("Scene Integration")]
        [SerializeField] private GameObject doorwayPrefab;
        [SerializeField] private Vector3 playerSpawnOffset = new Vector3(0f, 1f, 0f);
        [SerializeField] private Vector3 flatSceneReturnPosition = new Vector3(10f, 0f, -10.6f);
        [SerializeField] [Min(0f)] private float doorwayCenterHeight = 2f;
        [SerializeField] private Vector3 exitDoorwayPositionOffset = Vector3.zero;
        [SerializeField] private Vector3 exitDoorwayScale = Vector3.one;

        [Header("Materials")]
        [SerializeField] private Material floorMaterial;
        [SerializeField] private Material wallMaterial;

        public MazeAlgorithmKind AlgorithmKind => algorithmKind;
        public int Width => Mathf.Max(2, width);
        public int Height => Mathf.Max(2, height);
        public float CellSize => Mathf.Max(2f, cellSize);
        public float FloorThickness => Mathf.Max(0.05f, floorThickness);
        public float WallThickness => Mathf.Max(0.05f, wallThickness);
        public float WallHeight => Mathf.Max(1f, wallHeight);
        public Vector3 MazeOrigin => mazeOrigin;
        public bool UseSeedOverride => useSeedOverride;
        public int SeedOverride => seedOverride;
        public int MinimumCoinCount => Mathf.Max(0, minimumCoinCount);
        public int MaximumCoinCount => Mathf.Max(MinimumCoinCount, maximumCoinCount);
        public float CoinHeight => Mathf.Max(0f, coinHeight);
        public GameObject CollectablePrefab => collectablePrefab;
        public GameObject DoorwayPrefab => doorwayPrefab;
        public Vector3 PlayerSpawnOffset => playerSpawnOffset;
        public Vector3 FlatSceneReturnPosition => flatSceneReturnPosition;
        public float DoorwayCenterHeight => Mathf.Max(0f, doorwayCenterHeight);
        public Vector3 ExitDoorwayPositionOffset => exitDoorwayPositionOffset;
        public Vector3 ExitDoorwayScale => new Vector3(
            Mathf.Max(0.1f, exitDoorwayScale.x),
            Mathf.Max(0.1f, exitDoorwayScale.y),
            Mathf.Max(0.1f, exitDoorwayScale.z));
        public Material FloorMaterial => floorMaterial;
        public Material WallMaterial => wallMaterial;

        public Vector3 GetMazeEntrySpawnPosition()
        {
            // Spawn in the middle of the start cell so the entry doorway can target
            // a stable location regardless of which seed generated the current layout.
            return mazeOrigin + new Vector3(0f, FloorThickness, 0f) + playerSpawnOffset;
        }
    }
}
