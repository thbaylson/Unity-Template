using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Persists save payloads in WebGL builds through the virtual filesystem and syncs with IndexedDB.
/// </summary>
public class WebFileSaveStorage : ISaveStorage
{
    private readonly string _rootDir;

    public WebFileSaveStorage(string rootDir = null)
    {
        _rootDir = string.IsNullOrWhiteSpace(rootDir) ? Application.persistentDataPath : rootDir;
        Directory.CreateDirectory(_rootDir);
        WebGLSaveFileSync.SyncFromPersistentStorage();
    }

    private string PathFor(string fileName) => Path.Combine(_rootDir, fileName);

    public bool Exists(string fileName)
    {
        WebGLSaveFileSync.SyncFromPersistentStorage();
        return File.Exists(PathFor(fileName));
    }

    public byte[] ReadAllBytes(string fileName)
    {
        WebGLSaveFileSync.SyncFromPersistentStorage();
        return File.ReadAllBytes(PathFor(fileName));
    }

    public void WriteAllBytes(string fileName, byte[] bytes)
    {
        var path = PathFor(fileName);
        var tempPath = path + ".tmp";

        Directory.CreateDirectory(_rootDir);

        try
        {
            File.WriteAllBytes(tempPath, bytes);

            if (File.Exists(path)) File.Delete(path);

            File.Move(tempPath, path);
            WebGLSaveFileSync.SyncToPersistentStorage();
        }
        catch (Exception)
        {
            if (File.Exists(tempPath))
            {
                try { File.Delete(tempPath); } catch { }
            }

            throw;
        }
    }

    public void Delete(string fileName)
    {
        var path = PathFor(fileName);
        if (File.Exists(path)) File.Delete(path);

        WebGLSaveFileSync.SyncToPersistentStorage();
    }
}
