namespace Template.Maze.Generation
{
    public interface IMazeGeneratorAlgorithm
    {
        MazeLayout Generate(MazeGenerationRequest request);
    }
}
