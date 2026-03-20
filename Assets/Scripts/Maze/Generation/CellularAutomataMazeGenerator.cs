using System;
using System.Collections.Generic;

namespace Template.Maze.Generation
{
    /// <summary>
    /// Evolves a seeded cellular automata field, then uses it to bias carving and open extra loops.
    /// </summary>
    public sealed class CellularAutomataMazeGenerator : IMazeGeneratorAlgorithm
    {
        private const int SimulationStepCount = 4;
        private const double InitialOpenChance = 0.58d;

        public MazeLayout Generate(MazeGenerationRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var random = new Random(request.Seed);
            var opennessMap = CreateOpennessMap(request.Width, request.Height, random);
            var opennessScores = BuildOpennessScores(opennessMap);
            var layout = new MazeLayout(request.Width, request.Height);

            MazeGenerationUtility.CarveDepthFirstSpanningTree(
                layout,
                random,
                (current, neighbors, selectionRandom) =>
                    SelectNeighbor(current, neighbors, selectionRandom, opennessMap, opennessScores));

            OpenAdditionalPassages(layout, random, opennessMap, opennessScores);
            MazeGenerationUtility.SetFarthestBoundaryExit(layout);

            return layout;
        }

        private static bool[,] CreateOpennessMap(int width, int height, Random random)
        {
            var cells = new bool[width, height];

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    cells[x, y] = random.NextDouble() < InitialOpenChance;
                }
            }

            cells[0, 0] = true;

            for (var step = 0; step < SimulationStepCount; step++)
            {
                var next = new bool[width, height];

                for (var x = 0; x < width; x++)
                {
                    for (var y = 0; y < height; y++)
                    {
                        var openNeighborCount = CountOpenNeighbors(cells, x, y);

                        // These thresholds preserve dense open regions while collapsing isolated noise.
                        next[x, y] = cells[x, y]
                            ? openNeighborCount >= 4
                            : openNeighborCount >= 5;
                    }
                }

                next[0, 0] = true;
                cells = next;
            }

            return cells;
        }

        private static int[,] BuildOpennessScores(bool[,] opennessMap)
        {
            var width = opennessMap.GetLength(0);
            var height = opennessMap.GetLength(1);
            var scores = new int[width, height];

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    scores[x, y] = CountOpenNeighbors(opennessMap, x, y);
                }
            }

            return scores;
        }

        private static int CountOpenNeighbors(bool[,] cells, int x, int y)
        {
            var width = cells.GetLength(0);
            var height = cells.GetLength(1);
            var openNeighborCount = 0;

            for (var offsetX = -1; offsetX <= 1; offsetX++)
            {
                for (var offsetY = -1; offsetY <= 1; offsetY++)
                {
                    if (offsetX == 0 && offsetY == 0) continue;

                    var neighborX = x + offsetX;
                    var neighborY = y + offsetY;
                    if (neighborX < 0 || neighborY < 0 || neighborX >= width || neighborY >= height) continue;
                    if (!cells[neighborX, neighborY]) continue;

                    openNeighborCount++;
                }
            }

            return openNeighborCount;
        }

        private static MazeNeighborStep SelectNeighbor(
            MazeCoordinate current,
            IReadOnlyList<MazeNeighborStep> neighbors,
            Random random,
            bool[,] opennessMap,
            int[,] opennessScores)
        {
            var bestScore = int.MinValue;
            var bestNeighbors = new List<MazeNeighborStep>(neighbors.Count);
            var currentIsOpen = opennessMap[current.X, current.Y];

            for (var index = 0; index < neighbors.Count; index++)
            {
                var neighbor = neighbors[index];
                var coordinate = neighbor.Coordinate;
                var neighborIsOpen = opennessMap[coordinate.X, coordinate.Y];
                var score = opennessScores[coordinate.X, coordinate.Y];

                if (neighborIsOpen)
                {
                    score += 4;
                }

                if (neighborIsOpen == currentIsOpen)
                {
                    score += 2;
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    bestNeighbors.Clear();
                }

                if (score == bestScore)
                {
                    bestNeighbors.Add(neighbor);
                }
            }

            return bestNeighbors[random.Next(bestNeighbors.Count)];
        }

        private static void OpenAdditionalPassages(
            MazeLayout layout,
            Random random,
            bool[,] opennessMap,
            int[,] opennessScores)
        {
            for (var x = 0; x < layout.Width; x++)
            {
                for (var y = 0; y < layout.Height; y++)
                {
                    var current = new MazeCoordinate(x, y);
                    TryOpenPassage(layout, current, MazeWallDirection.East, random, opennessMap, opennessScores);
                    TryOpenPassage(layout, current, MazeWallDirection.South, random, opennessMap, opennessScores);
                }
            }
        }

        private static void TryOpenPassage(
            MazeLayout layout,
            MazeCoordinate current,
            MazeWallDirection direction,
            Random random,
            bool[,] opennessMap,
            int[,] opennessScores)
        {
            var next = current + MazeDirectionUtility.GetOffset(direction);
            if (!layout.Contains(next)) return;
            if (!layout.HasWall(current, direction)) return;
            if (!ShouldOpenPassage(current, next, random, opennessMap, opennessScores)) return;

            layout.CarvePassage(current, next, direction);
        }

        private static bool ShouldOpenPassage(
            MazeCoordinate current,
            MazeCoordinate next,
            Random random,
            bool[,] opennessMap,
            int[,] opennessScores)
        {
            var currentIsOpen = opennessMap[current.X, current.Y];
            var nextIsOpen = opennessMap[next.X, next.Y];
            var combinedScore = opennessScores[current.X, current.Y] + opennessScores[next.X, next.Y];

            // High-similarity cells should blend into larger chambers, while mixed regions
            // stay tighter so the result still reads as a maze instead of an open field.
            if (currentIsOpen && nextIsOpen)
            {
                return combinedScore >= 9 && random.NextDouble() < 0.8d;
            }

            if (currentIsOpen || nextIsOpen)
            {
                return combinedScore >= 11 && random.NextDouble() < 0.3d;
            }

            return false;
        }
    }
}
