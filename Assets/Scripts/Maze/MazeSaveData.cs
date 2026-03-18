using System;
using System.Collections.Generic;

namespace Template.Maze
{
    [Serializable]
    public class MazeSaveData
    {
        public int schemaVersion = 1;
        public bool hasActiveMaze;
        public int activeSeed;
        public bool isCompleted;
        public List<string> collectedCoinIds = new List<string>();

        public static MazeSaveData CreateEmpty(int schemaVersion)
        {
            return new MazeSaveData
            {
                schemaVersion = schemaVersion
            };
        }
    }

    public readonly struct MazeSessionState
    {
        public MazeSessionState(int seed, bool isCompleted, IReadOnlyCollection<string> collectedCoinIds)
        {
            Seed = seed;
            IsCompleted = isCompleted;
            CollectedCoinIds = collectedCoinIds ?? Array.Empty<string>();
        }

        public int Seed { get; }
        public bool IsCompleted { get; }
        public IReadOnlyCollection<string> CollectedCoinIds { get; }
    }
}
