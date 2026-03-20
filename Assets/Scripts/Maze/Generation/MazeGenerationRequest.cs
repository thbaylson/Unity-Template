namespace Template.Maze.Generation
{
    public sealed class MazeGenerationRequest
    {
        public MazeGenerationRequest(int width, int height, int seed)
        {
            Width = width;
            Height = height;
            Seed = seed;
        }

        public int Width { get; }
        public int Height { get; }
        public int Seed { get; }
    }
}
