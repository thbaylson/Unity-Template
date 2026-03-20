using System;

namespace Template.Maze.Generation
{
    public readonly struct MazeCoordinate : IEquatable<MazeCoordinate>
    {
        public MazeCoordinate(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; }
        public int Y { get; }

        public static MazeCoordinate operator +(MazeCoordinate left, MazeCoordinate right)
        {
            return new MazeCoordinate(left.X + right.X, left.Y + right.Y);
        }

        public bool Equals(MazeCoordinate other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is MazeCoordinate other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }
    }
}
