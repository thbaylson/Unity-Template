namespace Template.Maze.Generation
{
    public static class MazeDirectionUtility
    {
        public static readonly MazeWallDirection[] AllDirections =
        {
            MazeWallDirection.North,
            MazeWallDirection.East,
            MazeWallDirection.South,
            MazeWallDirection.West
        };

        public static MazeCoordinate GetOffset(MazeWallDirection direction)
        {
            switch (direction)
            {
                case MazeWallDirection.North:
                    return new MazeCoordinate(0, -1);
                case MazeWallDirection.East:
                    return new MazeCoordinate(1, 0);
                case MazeWallDirection.South:
                    return new MazeCoordinate(0, 1);
                default:
                    return new MazeCoordinate(-1, 0);
            }
        }

        public static MazeWallDirection GetOpposite(MazeWallDirection direction)
        {
            switch (direction)
            {
                case MazeWallDirection.North:
                    return MazeWallDirection.South;
                case MazeWallDirection.East:
                    return MazeWallDirection.West;
                case MazeWallDirection.South:
                    return MazeWallDirection.North;
                default:
                    return MazeWallDirection.East;
            }
        }
    }
}
