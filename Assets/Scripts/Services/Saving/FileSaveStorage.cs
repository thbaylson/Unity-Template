using System;
using System.IO;
using UnityEngine;

public interface ISaveStorage
{
    bool Exists(string fileName);
    byte[] ReadAllBytes(string fileName);
    void WriteAllBytes(string fileName, byte[] bytes);
    void Delete(string fileName);
}

public class FileSaveStorage : ISaveStorage
{
    private readonly string _rootDir;

    public FileSaveStorage(string rootDir = null)
    {
        _rootDir = string.IsNullOrWhiteSpace(rootDir) ? Application.persistentDataPath : rootDir;
        Directory.CreateDirectory(_rootDir);
    }

    private string PathFor(string fileName) => Path.Combine(_rootDir, fileName);

    public bool Exists(string fileName) => File.Exists(PathFor(fileName));

    public byte[] ReadAllBytes(string fileName) => File.ReadAllBytes(PathFor(fileName));

    public void WriteAllBytes(string fileName, byte[] bytes)
    {
        var path = PathFor(fileName);
        var tmp = path + ".tmp";

        try
        {
            File.WriteAllBytes(tmp, bytes);

            if (File.Exists(path)) File.Delete(path);

            File.Move(tmp, path);
        }
        catch (Exception)
        {
            if (File.Exists(tmp))
            {
                try { File.Delete(tmp); } catch { }
            }
            throw;
        }
    }

    public void Delete(string fileName)
    {
        var path = PathFor(fileName);
        if (File.Exists(path)) File.Delete(path);
    }
}
