using System;
using System.Collections.Generic;

namespace Template.Maze.Generation
{
    /// <summary>
    /// Provides shared carving and post-processing helpers used by maze generators.
    /// </summary>
    internal static class MazeGenerationUtility
    {
        public static void CarveDepthFirstSpanningTree(
            MazeLayout layout,
            Random random,
            Func<MazeCoordinate, IReadOnlyList<MazeNeighborStep>, Random, MazeNeighborStep> selectNeighbor)
        {
            if (layout == null) throw new ArgumentNullException(nameof(layout));
            if (random == null) throw new ArgumentNullException(nameof(random));
            if (selectNeighbor == null) throw new ArgumentNullException(nameof(selectNeighbor));

            var visited = new bool[layout.Width, layout.Height];
            var stack = new Stack<MazeCoordinate>();
            var start = layout.StartCoordinate;

            layout.SetFloor(start, true);
            visited[start.X, start.Y] = true;
            stack.Push(start);

            while (stack.Count > 0)
            {
                var current = stack.Peek();
                var neighbors = GetUnvisitedNeighbors(current, layout, visited);

                if (neighbors.Count == 0)
                {
                    stack.Pop();
                    continue;
                }

                var nextStep = selectNeighbor(current, neighbors, random);
                layout.CarvePassage(current, nextStep.Coordinate, nextStep.Direction);
                visited[nextStep.Coordinate.X, nextStep.Coordinate.Y] = true;
                stack.Push(nextStep.Coordinate);
            }
        }

        public static List<MazeNeighborStep> GetUnvisitedNeighbors(
            MazeCoordinate current,
            MazeLayout layout,
            bool[,] visited)
        {
            var result = new List<MazeNeighborStep>(4);

            foreach (var direction in MazeDirectionUtility.AllDirections)
            {
                var next = current + MazeDirectionUtility.GetOffset(direction);
                if (!layout.Contains(next)) continue;
                if (visited[next.X, next.Y]) continue;

                result.Add(new MazeNeighborStep(next, direction));
            }

            return result;
        }

        public static void CarveRectangularRoom(
            MazeLayout layout,
            int left,
            int top,
            int width,
            int height)
        {
            if (layout == null) throw new ArgumentNullException(nameof(layout));
            if (width <= 0 || height <= 0) return;

            var maxX = Math.Min(layout.Width - 1, left + width - 1);
            var maxY = Math.Min(layout.Height - 1, top + height - 1);

            for (var x = left; x <= maxX; x++)
            {
                for (var y = top; y <= maxY; y++)
                {
                    var current = new MazeCoordinate(x, y);
                    layout.SetFloor(current, true);

                    if (x < maxX)
                    {
                        layout.CarvePassage(current, new MazeCoordinate(x + 1, y), MazeWallDirection.East);
                    }

                    if (y < maxY)
                    {
                        layout.CarvePassage(current, new MazeCoordinate(x, y + 1), MazeWallDirection.South);
                    }
                }
            }
        }

        public static void CarveOrthogonalCorridor(
            MazeLayout layout,
            MazeCoordinate from,
            MazeCoordinate to,
            bool horizontalFirst)
        {
            if (layout == null) throw new ArgumentNullException(nameof(layout));

            layout.SetFloor(from, true);
            layout.SetFloor(to, true);
            var current = from;

            if (horizontalFirst)
            {
                current = CarveHorizontalSegment(layout, current, to.X);
                CarveVerticalSegment(layout, current, to.Y);
                return;
            }

            current = CarveVerticalSegment(layout, current, to.Y);
            CarveHorizontalSegment(layout, current, to.X);
        }

        public static void SetFarthestBoundaryExit(MazeLayout layout)
        {
            if (layout == null) throw new ArgumentNullException(nameof(layout));

            var distances = BuildDistanceGrid(layout);
            var bestDistance = int.MinValue;
            var bestCoordinate = layout.StartCoordinate;
            var bestDirection = MazeWallDirection.East;

            for (var x = 0; x < layout.Width; x++)
            {
                for (var y = 0; y < layout.Height; y++)
                {
                    var coordinate = new MazeCoordinate(x, y);
                    if (coordinate.Equals(layout.StartCoordinate)) continue;
                    if (!TryGetBoundaryExitDirection(coordinate, layout.Width, layout.Height, out var direction)) continue;

                    var distance = distances[x, y];
                    if (distance < 0 || distance <= bestDistance) continue;

                    bestDistance = distance;
                    bestCoordinate = coordinate;
                    bestDirection = direction;
                }
            }

            layout.SetExit(bestCoordinate, bestDirection);
        }

        public static int[,] BuildDistanceGrid(MazeLayout layout)
        {
            if (layout == null) throw new ArgumentNullException(nameof(layout));

            var distances = new int[layout.Width, layout.Height];
            for (var x = 0; x < layout.Width; x++)
            {
                for (var y = 0; y < layout.Height; y++)
                {
                    distances[x, y] = -1;
                }
            }

            if (!layout.HasFloor(layout.StartCoordinate))
            {
                return distances;
            }

            var queue = new Queue<MazeCoordinate>();
            queue.Enqueue(layout.StartCoordinate);
            distances[layout.StartCoordinate.X, layout.StartCoordinate.Y] = 0;

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var nextDistance = distances[current.X, current.Y] + 1;

                foreach (var neighbor in layout.GetReachableNeighbors(current))
                {
                    if (distances[neighbor.X, neighbor.Y] >= 0) continue;

                    distances[neighbor.X, neighbor.Y] = nextDistance;
                    queue.Enqueue(neighbor);
                }
            }

            return distances;
        }

        public static bool TryGetBoundaryExitDirection(
            MazeCoordinate coordinate,
            int width,
            int height,
            out MazeWallDirection direction)
        {
            if (coordinate.X == width - 1)
            {
                direction = MazeWallDirection.East;
                return true;
            }

            if (coordinate.Y == height - 1)
            {
                direction = MazeWallDirection.South;
                return true;
            }

            if (coordinate.X == 0)
            {
                direction = MazeWallDirection.West;
                return true;
            }

            if (coordinate.Y == 0)
            {
                direction = MazeWallDirection.North;
                return true;
            }

            direction = MazeWallDirection.East;
            return false;
        }

        private static MazeCoordinate CarveHorizontalSegment(MazeLayout layout, MazeCoordinate start, int targetX)
        {
            var current = start;

            while (current.X != targetX)
            {
                var direction = current.X < targetX
                    ? MazeWallDirection.East
                    : MazeWallDirection.West;
                var next = current + MazeDirectionUtility.GetOffset(direction);
                layout.CarvePassage(current, next, direction);
                current = next;
            }

            return current;
        }

        private static MazeCoordinate CarveVerticalSegment(MazeLayout layout, MazeCoordinate start, int targetY)
        {
            var current = start;

            while (current.Y != targetY)
            {
                var direction = current.Y < targetY
                    ? MazeWallDirection.South
                    : MazeWallDirection.North;
                var next = current + MazeDirectionUtility.GetOffset(direction);
                layout.CarvePassage(current, next, direction);
                current = next;
            }

            return current;
        }
    }

    internal readonly struct MazeNeighborStep
    {
        public MazeNeighborStep(MazeCoordinate coordinate, MazeWallDirection direction)
        {
            Coordinate = coordinate;
            Direction = direction;
        }

        public MazeCoordinate Coordinate { get; }
        public MazeWallDirection Direction { get; }
    }
}
