using System.Collections.Generic;

namespace Template.Maze.Generation
{
    public sealed class MazeLayout
    {
        private readonly MazeCell[,] _cells;

        public MazeLayout(int width, int height)
        {
            Width = width;
            Height = height;
            StartCoordinate = new MazeCoordinate(0, 0);
            ExitCoordinate = StartCoordinate;
            ExitDirection = MazeWallDirection.East;

            _cells = new MazeCell[width, height];
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    _cells[x, y] = MazeCell.CreateDefault();
                }
            }
        }

        public int Width { get; }
        public int Height { get; }
        public MazeCoordinate StartCoordinate { get; }
        public MazeCoordinate ExitCoordinate { get; private set; }
        public MazeWallDirection ExitDirection { get; private set; }

        public MazeCell GetCell(int x, int y)
        {
            return _cells[x, y];
        }

        public bool Contains(MazeCoordinate coordinate)
        {
            return coordinate.X >= 0 &&
                   coordinate.Y >= 0 &&
                   coordinate.X < Width &&
                   coordinate.Y < Height;
        }

        public bool HasFloor(MazeCoordinate coordinate)
        {
            return GetCell(coordinate.X, coordinate.Y).HasFloor;
        }

        public void SetCell(MazeCoordinate coordinate, MazeCell cell)
        {
            _cells[coordinate.X, coordinate.Y] = cell;
        }

        public void SetFloor(MazeCoordinate coordinate, bool hasFloor)
        {
            var cell = GetCell(coordinate.X, coordinate.Y);
            cell.SetFloor(hasFloor);
            SetCell(coordinate, cell);
        }

        public void CarvePassage(MazeCoordinate from, MazeCoordinate to, MazeWallDirection direction)
        {
            var fromCell = GetCell(from.X, from.Y);
            fromCell.SetFloor(true);
            fromCell.ClearWall(direction);
            SetCell(from, fromCell);

            var toCell = GetCell(to.X, to.Y);
            toCell.SetFloor(true);
            toCell.ClearWall(MazeDirectionUtility.GetOpposite(direction));
            SetCell(to, toCell);
        }

        public bool HasWall(MazeCoordinate coordinate, MazeWallDirection direction)
        {
            return GetCell(coordinate.X, coordinate.Y).HasWall(direction);
        }

        public void SetExit(MazeCoordinate coordinate, MazeWallDirection direction)
        {
            var cell = GetCell(coordinate.X, coordinate.Y);
            cell.SetFloor(true);
            cell.ClearWall(direction);
            SetCell(coordinate, cell);

            ExitCoordinate = coordinate;
            ExitDirection = direction;
        }

        public IEnumerable<MazeCoordinate> GetReachableNeighbors(MazeCoordinate coordinate)
        {
            if (!HasFloor(coordinate)) yield break;

            foreach (var direction in MazeDirectionUtility.AllDirections)
            {
                if (HasWall(coordinate, direction)) continue;

                var next = coordinate + MazeDirectionUtility.GetOffset(direction);
                if (!Contains(next)) continue;
                if (!HasFloor(next)) continue;

                yield return next;
            }
        }
    }
}
