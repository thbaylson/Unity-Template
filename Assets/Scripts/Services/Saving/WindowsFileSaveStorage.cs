using System;
using System.IO;
using UnityEngine;

namespace Template.Services.Saving
{
    /// <summary>
    /// Persists save payloads to platform filesystem paths backed by Application.persistentDataPath.
    /// </summary>
    public class WindowsFileSaveStorage : ISaveStorage
    {
        private readonly string _rootDir;

        public WindowsFileSaveStorage(string rootDir = null)
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
            var tempPath = path + ".tmp";

            try
            {
                File.WriteAllBytes(tempPath, bytes);

                if (File.Exists(path)) File.Delete(path);

                File.Move(tempPath, path);
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
        }
    }
}
