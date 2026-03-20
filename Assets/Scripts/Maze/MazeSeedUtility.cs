using System;

namespace Template.Maze
{
    public static class MazeSeedUtility
    {
        public static int CreateRandomSeed()
        {
            unchecked
            {
                return NormalizeSeed(Guid.NewGuid().GetHashCode() ^ Environment.TickCount);
            }
        }

        public static int GetNextSeed(int currentSeed)
        {
            unchecked
            {
                uint hash = (uint)currentSeed;
                hash ^= 2747636419u;
                hash *= 2654435769u;
                hash ^= hash >> 16;
                hash *= 2654435769u;
                hash ^= hash >> 16;
                hash *= 2654435769u;
                return NormalizeSeed((int)hash);
            }
        }

        public static int Derive(int seed, int salt)
        {
            return GetNextSeed(unchecked(seed ^ salt));
        }

        private static int NormalizeSeed(int seed)
        {
            return seed == 0 ? 1 : seed;
        }
    }
}
