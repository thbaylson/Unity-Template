namespace Template.Maze.Generation
{
    public struct MazeCell
    {
        public bool HasFloor;
        public bool HasNorthWall;
        public bool HasEastWall;
        public bool HasSouthWall;
        public bool HasWestWall;

        public static MazeCell CreateDefault()
        {
            return new MazeCell
            {
                HasFloor = false,
                HasNorthWall = true,
                HasEastWall = true,
                HasSouthWall = true,
                HasWestWall = true
            };
        }

        public void SetFloor(bool hasFloor)
        {
            HasFloor = hasFloor;
        }

        public bool HasWall(MazeWallDirection direction)
        {
            switch (direction)
            {
                case MazeWallDirection.North:
                    return HasNorthWall;
                case MazeWallDirection.East:
                    return HasEastWall;
                case MazeWallDirection.South:
                    return HasSouthWall;
                default:
                    return HasWestWall;
            }
        }

        public void ClearWall(MazeWallDirection direction)
        {
            switch (direction)
            {
                case MazeWallDirection.North:
                    HasNorthWall = false;
                    break;
                case MazeWallDirection.East:
                    HasEastWall = false;
                    break;
                case MazeWallDirection.South:
                    HasSouthWall = false;
                    break;
                default:
                    HasWestWall = false;
                    break;
            }
        }
    }
}
