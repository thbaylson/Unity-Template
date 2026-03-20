using System.Reflection;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Guards the maze-entry spawn calculation so the start cell stays centered on the configured origin.
/// </summary>
public class MazeConfigTests
{
    [Test]
    public void GetMazeEntrySpawnPosition_CentersStartCellOnOrigin()
    {
        var config = ScriptableObject.CreateInstance<Template.Maze.MazeConfig>();

        try
        {
            SetField(config, "mazeOrigin", new Vector3(12f, 0f, -8f));
            SetField(config, "floorThickness", 0.25f);
            SetField(config, "playerSpawnOffset", new Vector3(0f, 1f, 0f));

            Assert.That(config.GetMazeEntrySpawnPosition(), Is.EqualTo(new Vector3(12f, 1.25f, -8f)));
        }
        finally
        {
            Object.DestroyImmediate(config);
        }
    }

    private static void SetField<T>(Template.Maze.MazeConfig config, string fieldName, T value)
    {
        var field = typeof(Template.Maze.MazeConfig).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(field, Is.Not.Null);
        field.SetValue(config, value);
    }
}
