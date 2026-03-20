using System;
using System.Collections.Generic;
using NUnit.Framework;
using Template.Maze;
using Template.Maze.Generation;

/// <summary>
/// Covers deterministic and traversible maze generation with fast pure C# tests.
/// </summary>
public class MazeGenerationTests
{
    private static IEnumerable<TestCaseData> Algorithms()
    {
        yield return new TestCaseData(MazeAlgorithmKind.RecursiveBacktracker)
            .SetName("Generate_WithSameSeed_ProducesSameLayout_RecursiveBacktracker");
        yield return new TestCaseData(MazeAlgorithmKind.CellularAutomata)
            .SetName("Generate_WithSameSeed_ProducesSameLayout_CellularAutomata");
        yield return new TestCaseData(MazeAlgorithmKind.RoomAndCorridorDungeon)
            .SetName("Generate_WithSameSeed_ProducesSameLayout_RoomAndCorridorDungeon");
    }

    private static IEnumerable<TestCaseData> FactoryAlgorithms()
    {
        yield return new TestCaseData(MazeAlgorithmKind.RecursiveBacktracker, typeof(RecursiveBacktrackerMazeGenerator));
        yield return new TestCaseData(MazeAlgorithmKind.CellularAutomata, typeof(CellularAutomataMazeGenerator));
        yield return new TestCaseData(MazeAlgorithmKind.RoomAndCorridorDungeon, typeof(RoomAndCorridorDungeonGenerator));
    }

    [TestCaseSource(nameof(Algorithms))]
    public void Generate_WithSameSeed_ProducesSameLayout(MazeAlgorithmKind algorithmKind)
    {
        var generator = MazeGeneratorFactory.Create(algorithmKind);

        var first = generator.Generate(new MazeGenerationRequest(8, 6, 123456));
        var second = generator.Generate(new MazeGenerationRequest(8, 6, 123456));

        Assert.That(second.ExitCoordinate, Is.EqualTo(first.ExitCoordinate));
        Assert.That(second.ExitDirection, Is.EqualTo(first.ExitDirection));

        for (var x = 0; x < first.Width; x++)
        {
            for (var y = 0; y < first.Height; y++)
            {
                var a = first.GetCell(x, y);
                var b = second.GetCell(x, y);

                Assert.That(b.HasFloor, Is.EqualTo(a.HasFloor));
                Assert.That(b.HasNorthWall, Is.EqualTo(a.HasNorthWall));
                Assert.That(b.HasEastWall, Is.EqualTo(a.HasEastWall));
                Assert.That(b.HasSouthWall, Is.EqualTo(a.HasSouthWall));
                Assert.That(b.HasWestWall, Is.EqualTo(a.HasWestWall));
            }
        }
    }

    [TestCaseSource(nameof(Algorithms))]
    public void Generate_KeepsEveryPlayableCellConnected(MazeAlgorithmKind algorithmKind)
    {
        var generator = MazeGeneratorFactory.Create(algorithmKind);
        var layout = generator.Generate(new MazeGenerationRequest(9, 7, 98765));
        var visited = GetVisitedCells(layout);

        Assert.That(visited.Contains(layout.ExitCoordinate), Is.True);

        for (var x = 0; x < layout.Width; x++)
        {
            for (var y = 0; y < layout.Height; y++)
            {
                var coordinate = new MazeCoordinate(x, y);
                if (!layout.HasFloor(coordinate)) continue;

                Assert.That(visited.Contains(coordinate), Is.True, $"Playable cell {coordinate} was disconnected.");
            }
        }
    }

    [TestCaseSource(nameof(FactoryAlgorithms))]
    public void Create_WithAlgorithmKind_ReturnsExpectedGenerator(MazeAlgorithmKind algorithmKind, Type expectedGeneratorType)
    {
        var generator = MazeGeneratorFactory.Create(algorithmKind);

        Assert.That(generator, Is.TypeOf(expectedGeneratorType));
    }

    [Test]
    public void Generate_WithRoomAndCorridorDungeonKind_CreatesAtLeastOneSquareRoom()
    {
        var generator = MazeGeneratorFactory.Create(MazeAlgorithmKind.RoomAndCorridorDungeon);
        var layout = generator.Generate(new MazeGenerationRequest(16, 12, 24680));

        Assert.That(CountOpenRoomSquares(layout), Is.GreaterThan(0));
    }

    [Test]
    public void Generate_WithRoomAndCorridorDungeonKind_LeavesSomeCellsSolid()
    {
        var generator = MazeGeneratorFactory.Create(MazeAlgorithmKind.RoomAndCorridorDungeon);
        var layout = generator.Generate(new MazeGenerationRequest(16, 12, 24680));
        var solidCellCount = 0;

        for (var x = 0; x < layout.Width; x++)
        {
            for (var y = 0; y < layout.Height; y++)
            {
                if (layout.HasFloor(new MazeCoordinate(x, y))) continue;

                solidCellCount++;
            }
        }

        Assert.That(solidCellCount, Is.GreaterThan(0));
    }

    [Test]
    public void CarvePassage_ActivatesBothEndpointCells()
    {
        var layout = new MazeLayout(2, 2);
        var start = new MazeCoordinate(0, 0);
        var next = new MazeCoordinate(1, 0);

        layout.CarvePassage(start, next, MazeWallDirection.East);

        Assert.That(layout.HasFloor(start), Is.True);
        Assert.That(layout.HasFloor(next), Is.True);
    }

    private static HashSet<MazeCoordinate> GetVisitedCells(MazeLayout layout)
    {
        var visited = new HashSet<MazeCoordinate>();
        var queue = new Queue<MazeCoordinate>();

        queue.Enqueue(layout.StartCoordinate);
        visited.Add(layout.StartCoordinate);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var neighbor in layout.GetReachableNeighbors(current))
            {
                if (!visited.Add(neighbor)) continue;
                queue.Enqueue(neighbor);
            }
        }

        return visited;
    }
    private static int CountOpenRoomSquares(MazeLayout layout)
    {
        var roomSquares = 0;

        for (var x = 0; x < layout.Width - 1; x++)
        {
            for (var y = 0; y < layout.Height - 1; y++)
            {
                var topLeft = new MazeCoordinate(x, y);
                var topRight = new MazeCoordinate(x + 1, y);
                var bottomLeft = new MazeCoordinate(x, y + 1);

                var isOpenSquare =
                    !layout.HasWall(topLeft, MazeWallDirection.East) &&
                    !layout.HasWall(topLeft, MazeWallDirection.South) &&
                    !layout.HasWall(topRight, MazeWallDirection.South) &&
                    !layout.HasWall(bottomLeft, MazeWallDirection.East);

                if (isOpenSquare)
                {
                    roomSquares++;
                }
            }
        }

        return roomSquares;
    }
}
