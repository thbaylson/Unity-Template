using System.Collections.Generic;
using NUnit.Framework;
using Template.Maze.Generation;

/// <summary>
/// Covers deterministic and traversible maze generation with fast pure C# tests.
/// </summary>
public class MazeGenerationTests
{
    [Test]
    public void Generate_WithSameSeed_ProducesSameLayout()
    {
        var generator = new RecursiveBacktrackerMazeGenerator();

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

    [Test]
    public void Generate_CreatesLayoutThatReachesEveryCell()
    {
        var generator = new RecursiveBacktrackerMazeGenerator();
        var layout = generator.Generate(new MazeGenerationRequest(9, 7, 98765));
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

        Assert.That(visited.Count, Is.EqualTo(layout.Width * layout.Height));
        Assert.That(visited.Contains(layout.ExitCoordinate), Is.True);
    }
}
