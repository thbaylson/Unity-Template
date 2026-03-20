using System.Collections.Generic;
using NUnit.Framework;
using Template.Services.Saving;

/// <summary>
/// Verifies maze state persists to its own save payload instead of the player save file.
/// </summary>
public class MazeSaveRepositoryTests
{
    [Test]
    public void SaveAndLoad_UsesDedicatedMazeSaveFile()
    {
        const string MazeFileName = "maze_save.json";
        const string PlayerFileName = "game_save.json";
        var storage = new InMemorySaveStorage();
        var repository = new Template.Maze.MazeSaveRepository(storage, new JsonSaveSerializer(), MazeFileName);

        var saveData = new Template.Maze.MazeSaveData
        {
            schemaVersion = 1,
            hasActiveMaze = true,
            activeSeed = 424242,
            isCompleted = false,
            collectedCoinIds = new List<string> { "coin_001", "coin_007" }
        };

        repository.Save(saveData, 1);
        var loaded = repository.Load(1);

        Assert.That(storage.Exists(MazeFileName), Is.True);
        Assert.That(storage.Exists(PlayerFileName), Is.False);
        Assert.That(loaded.hasActiveMaze, Is.True);
        Assert.That(loaded.activeSeed, Is.EqualTo(424242));
        Assert.That(loaded.isCompleted, Is.False);
        Assert.That(loaded.collectedCoinIds, Is.EquivalentTo(new[] { "coin_001", "coin_007" }));
    }

    private sealed class InMemorySaveStorage : ISaveStorage
    {
        private readonly Dictionary<string, byte[]> _files = new Dictionary<string, byte[]>();

        public bool Exists(string fileName)
        {
            return _files.ContainsKey(fileName);
        }

        public byte[] ReadAllBytes(string fileName)
        {
            return _files[fileName];
        }

        public void WriteAllBytes(string fileName, byte[] bytes)
        {
            _files[fileName] = bytes;
        }

        public void Delete(string fileName)
        {
            _files.Remove(fileName);
        }
    }
}
