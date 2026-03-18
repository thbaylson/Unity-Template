namespace Template.Maze.Generation
{
    public static class MazeGeneratorFactory
    {
        public static IMazeGeneratorAlgorithm Create(MazeAlgorithmKind algorithmKind)
        {
            switch (algorithmKind)
            {
                case MazeAlgorithmKind.RecursiveBacktracker:
                default:
                    return new RecursiveBacktrackerMazeGenerator();
            }
        }
    }
}
