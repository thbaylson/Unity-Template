using System.Collections.Generic;
using Template.Services.Saving;

namespace Template.Maze
{
    public class MazeSaveRepository
    {
        private readonly ISaveStorage _storage;
        private readonly ISaveSerializer _serializer;
        private readonly string _fileName;

        public MazeSaveRepository(ISaveStorage storage, ISaveSerializer serializer, string fileName)
        {
            _storage = storage;
            _serializer = serializer;
            _fileName = fileName;
        }

        public MazeSaveData Load(int expectedSchemaVersion)
        {
            if (_storage == null || _serializer == null || string.IsNullOrWhiteSpace(_fileName))
            {
                return MazeSaveData.CreateEmpty(expectedSchemaVersion);
            }

            if (!_storage.Exists(_fileName))
            {
                return MazeSaveData.CreateEmpty(expectedSchemaVersion);
            }

            try
            {
                var bytes = _storage.ReadAllBytes(_fileName);
                var saveData = _serializer.Deserialize<MazeSaveData>(bytes);

                if (saveData == null || saveData.schemaVersion != expectedSchemaVersion)
                {
                    return MazeSaveData.CreateEmpty(expectedSchemaVersion);
                }

                if (saveData.collectedCoinIds == null)
                {
                    saveData.collectedCoinIds = new List<string>();
                }

                return saveData;
            }
            catch
            {
                return MazeSaveData.CreateEmpty(expectedSchemaVersion);
            }
        }

        public void Save(MazeSaveData saveData, int expectedSchemaVersion)
        {
            if (_storage == null || _serializer == null || string.IsNullOrWhiteSpace(_fileName) || saveData == null)
            {
                return;
            }

            saveData.schemaVersion = expectedSchemaVersion;
            if (saveData.collectedCoinIds == null)
            {
                saveData.collectedCoinIds = new List<string>();
            }

            var bytes = _serializer.Serialize(saveData);
            _storage.WriteAllBytes(_fileName, bytes);
        }

        public void Delete()
        {
            if (_storage == null || string.IsNullOrWhiteSpace(_fileName))
            {
                return;
            }

            _storage.Delete(_fileName);
        }
    }
}
