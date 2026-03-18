using System;
using System.Collections.Generic;

namespace Template.Maze.Generation
{
    public sealed class RecursiveBacktrackerMazeGenerator : IMazeGeneratorAlgorithm
    {
        public MazeLayout Generate(MazeGenerationRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var layout = new MazeLayout(request.Width, request.Height);
            var visited = new bool[request.Width, request.Height];
            var random = new Random(request.Seed);
            var stack = new Stack<MazeCoordinate>();
            var start = layout.StartCoordinate;

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

                var nextStep = neighbors[random.Next(neighbors.Count)];
                layout.CarvePassage(current, nextStep.Coordinate, nextStep.Direction);
                visited[nextStep.Coordinate.X, nextStep.Coordinate.Y] = true;
                stack.Push(nextStep.Coordinate);
            }

            var exitStep = FindFarthestBoundaryCell(layout);
            layout.SetExit(exitStep.Coordinate, exitStep.Direction);

            return layout;
        }

        private static List<NeighborStep> GetUnvisitedNeighbors(
            MazeCoordinate current,
            MazeLayout layout,
            bool[,] visited)
        {
            var result = new List<NeighborStep>(4);

            foreach (var direction in MazeDirectionUtility.AllDirections)
            {
                var next = current + MazeDirectionUtility.GetOffset(direction);
                if (!layout.Contains(next)) continue;
                if (visited[next.X, next.Y]) continue;

                result.Add(new NeighborStep(next, direction));
            }

            return result;
        }

        private static ExitStep FindFarthestBoundaryCell(MazeLayout layout)
        {
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
                    if (distance <= bestDistance) continue;

                    bestDistance = distance;
                    bestCoordinate = coordinate;
                    bestDirection = direction;
                }
            }

            return new ExitStep(bestCoordinate, bestDirection);
        }

        private static int[,] BuildDistanceGrid(MazeLayout layout)
        {
            var distances = new int[layout.Width, layout.Height];
            for (var x = 0; x < layout.Width; x++)
            {
                for (var y = 0; y < layout.Height; y++)
                {
                    distances[x, y] = -1;
                }
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

        private static bool TryGetBoundaryExitDirection(
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

        private readonly struct NeighborStep
        {
            public NeighborStep(MazeCoordinate coordinate, MazeWallDirection direction)
            {
                Coordinate = coordinate;
                Direction = direction;
            }

            public MazeCoordinate Coordinate { get; }
            public MazeWallDirection Direction { get; }
        }

        private readonly struct ExitStep
        {
            public ExitStep(MazeCoordinate coordinate, MazeWallDirection direction)
            {
                Coordinate = coordinate;
                Direction = direction;
            }

            public MazeCoordinate Coordinate { get; }
            public MazeWallDirection Direction { get; }
        }
    }
}
