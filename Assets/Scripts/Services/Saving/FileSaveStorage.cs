using System;
using System.IO;
using UnityEngine;

public interface ISaveStorage
{
    bool Exists(string fileName);
    byte[] ReadAllBytes(string fileName);
    void WriteAllBytes(string fileName, byte[] bytes);
    void Delete(string fileName);
    void SyncFromPersistentStorage();
    void SyncToPersistentStorage();
}

public class FileSaveStorage : ISaveStorage
{
#if UNITY_WEBGL && !UNITY_EDITOR
    private const string WebGlSavePrefix = "webgl.save.";
#else
    private readonly string _rootDir;
#endif

    public FileSaveStorage(string rootDir = null)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
#else
        _rootDir = string.IsNullOrWhiteSpace(rootDir) ? Application.persistentDataPath : rootDir;
        Directory.CreateDirectory(_rootDir);
#endif
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    private static string PlayerPrefsKeyFor(string fileName) => WebGlSavePrefix + fileName;
#else
    private string PathFor(string fileName) => Path.Combine(_rootDir, fileName);
#endif

    public bool Exists(string fileName)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return PlayerPrefs.HasKey(PlayerPrefsKeyFor(fileName));
#else
        return File.Exists(PathFor(fileName));
#endif
    }

    public byte[] ReadAllBytes(string fileName)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        var encodedSaveData = PlayerPrefs.GetString(PlayerPrefsKeyFor(fileName), string.Empty);
        return Convert.FromBase64String(encodedSaveData);
#else
        return File.ReadAllBytes(PathFor(fileName));
#endif
    }

    public void WriteAllBytes(string fileName, byte[] bytes)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        var encodedSaveData = Convert.ToBase64String(bytes);
        PlayerPrefs.SetString(PlayerPrefsKeyFor(fileName), encodedSaveData);
        PlayerPrefs.Save();
#else
        var path = PathFor(fileName);
        var tempPath = path + ".tmp";

        try
        {
            File.WriteAllBytes(tempPath, bytes);

            if (File.Exists(path)) File.Delete(path);

            File.Move(tempPath, path);

            SyncToPersistentStorage();
        }
        catch (Exception)
        {
            if (File.Exists(tempPath))
            {
                try { File.Delete(tempPath); } catch { }
            }
            throw;
        }
#endif
    }

    public void Delete(string fileName)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        PlayerPrefs.DeleteKey(PlayerPrefsKeyFor(fileName));
        PlayerPrefs.Save();
#else
        var path = PathFor(fileName);
        if (File.Exists(path)) File.Delete(path);

        SyncToPersistentStorage();
#endif
    }

    public void SyncFromPersistentStorage()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return;
#else
        WebGLSaveFileSync.SyncFromPersistentStorage();
#endif
    }

    public void SyncToPersistentStorage()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        return;
#else
        WebGLSaveFileSync.SyncToPersistentStorage();
#endif
    }
}
