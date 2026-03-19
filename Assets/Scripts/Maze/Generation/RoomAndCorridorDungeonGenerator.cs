using System;
using System.Collections.Generic;

namespace Template.Maze.Generation
{
    /// <summary>
    /// Builds a simple dungeon by carving square rooms and L-shaped corridors between them.
    /// </summary>
    public sealed class RoomAndCorridorDungeonGenerator : IMazeGeneratorAlgorithm
    {
        private const int AnchorRoomSize = 2;
        private const int MinRoomSize = 2;
        private const int MaxRoomSize = 4;

        public MazeLayout Generate(MazeGenerationRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            var random = new Random(request.Seed);
            var layout = new MazeLayout(request.Width, request.Height);
            var rooms = CreateRooms(request.Width, request.Height, random);

            for (var index = 0; index < rooms.Count; index++)
            {
                var room = rooms[index];
                MazeGenerationUtility.CarveRectangularRoom(layout, room.Left, room.Top, room.Size, room.Size);
            }

            ConnectRooms(layout, rooms, random);
            MazeGenerationUtility.SetFarthestBoundaryExit(layout);

            return layout;
        }

        private static List<DungeonRoom> CreateRooms(int width, int height, Random random)
        {
            var rooms = new List<DungeonRoom>();
            var anchorSize = Math.Min(AnchorRoomSize, Math.Min(width, height));

            rooms.Add(new DungeonRoom(0, 0, anchorSize));

            var exitRoom = new DungeonRoom(width - anchorSize, height - anchorSize, anchorSize);
            if (!OverlapsExisting(exitRoom, rooms, 0))
            {
                rooms.Add(exitRoom);
            }

            var maxRoomSize = Math.Min(MaxRoomSize, Math.Min(width, height));
            if (maxRoomSize < MinRoomSize)
            {
                return rooms;
            }

            var targetRoomCount = Math.Max(2, (width * height) / 24);
            var attemptCount = Math.Max(targetRoomCount * 4, 8);

            for (var attempt = 0; attempt < attemptCount; attempt++)
            {
                var size = random.Next(MinRoomSize, maxRoomSize + 1);
                var candidate = new DungeonRoom(
                    random.Next(width - size + 1),
                    random.Next(height - size + 1),
                    size);

                if (OverlapsExisting(candidate, rooms, 1))
                {
                    continue;
                }

                rooms.Add(candidate);
                if (rooms.Count >= targetRoomCount)
                {
                    break;
                }
            }

            return rooms;
        }

        private static bool OverlapsExisting(DungeonRoom candidate, IReadOnlyList<DungeonRoom> rooms, int padding)
        {
            for (var index = 0; index < rooms.Count; index++)
            {
                if (candidate.Overlaps(rooms[index], padding))
                {
                    return true;
                }
            }

            return false;
        }

        private static void ConnectRooms(MazeLayout layout, IReadOnlyList<DungeonRoom> rooms, Random random)
        {
            if (rooms == null || rooms.Count == 0)
            {
                return;
            }

            var connectedRooms = new List<DungeonRoom> { rooms[0] };
            var remainingRooms = new List<DungeonRoom>();

            for (var index = 1; index < rooms.Count; index++)
            {
                remainingRooms.Add(rooms[index]);
            }

            while (remainingRooms.Count > 0)
            {
                var bestDistance = int.MaxValue;
                var bestConnectedRoom = connectedRooms[0];
                var bestRemainingIndex = 0;

                for (var connectedIndex = 0; connectedIndex < connectedRooms.Count; connectedIndex++)
                {
                    var connectedRoom = connectedRooms[connectedIndex];

                    for (var remainingIndex = 0; remainingIndex < remainingRooms.Count; remainingIndex++)
                    {
                        var remainingRoom = remainingRooms[remainingIndex];
                        var distance = GetManhattanDistance(connectedRoom.Center, remainingRoom.Center);
                        if (distance >= bestDistance)
                        {
                            continue;
                        }

                        bestDistance = distance;
                        bestConnectedRoom = connectedRoom;
                        bestRemainingIndex = remainingIndex;
                    }
                }

                var nextRoom = remainingRooms[bestRemainingIndex];
                MazeGenerationUtility.CarveOrthogonalCorridor(
                    layout,
                    bestConnectedRoom.Center,
                    nextRoom.Center,
                    random.Next(2) == 0);

                connectedRooms.Add(nextRoom);
                remainingRooms.RemoveAt(bestRemainingIndex);
            }
        }

        private static int GetManhattanDistance(MazeCoordinate left, MazeCoordinate right)
        {
            return Math.Abs(left.X - right.X) + Math.Abs(left.Y - right.Y);
        }

        private readonly struct DungeonRoom
        {
            public DungeonRoom(int left, int top, int size)
            {
                Left = left;
                Top = top;
                Size = size;
            }

            public int Left { get; }
            public int Top { get; }
            public int Size { get; }
            public int Right => Left + Size - 1;
            public int Bottom => Top + Size - 1;
            public MazeCoordinate Center => new MazeCoordinate(
                Math.Min(Right, Left + (Size / 2)),
                Math.Min(Bottom, Top + (Size / 2)));

            public bool Overlaps(DungeonRoom other, int padding)
            {
                return Left <= other.Right + padding &&
                       Right + padding >= other.Left &&
                       Top <= other.Bottom + padding &&
                       Bottom + padding >= other.Top;
            }
        }
    }
}
