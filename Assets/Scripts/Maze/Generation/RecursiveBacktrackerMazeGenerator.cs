using System;

namespace Template.Maze.Generation
{
    public sealed class RecursiveBacktrackerMazeGenerator : IMazeGeneratorAlgorithm
    {
        public MazeLayout Generate(MazeGenerationRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var layout = new MazeLayout(request.Width, request.Height);
            var random = new Random(request.Seed);
            MazeGenerationUtility.CarveDepthFirstSpanningTree(layout, random, SelectRandomNeighbor);
            MazeGenerationUtility.SetFarthestBoundaryExit(layout);

            return layout;
        }

        private static MazeNeighborStep SelectRandomNeighbor(
            MazeCoordinate current,
            System.Collections.Generic.IReadOnlyList<MazeNeighborStep> neighbors,
            Random random)
        {
            return neighbors[random.Next(neighbors.Count)];
        }
    }
}
