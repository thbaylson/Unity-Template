using System;
using System.Collections.Generic;

namespace Template.Services.Saving
{
    [Serializable]
    public class GameFileDto
    {
        public int schemaVersion = 1;
        public PlayerSaveData player = new PlayerSaveData();
        public List<SceneFlagsDto> scenes = new List<SceneFlagsDto>();
    }

    [Serializable]
    public class SceneFlagsDto
    {
        public string sceneName = "";
        public List<FlagEntryDto> flags = new List<FlagEntryDto>();
    }

    [Serializable]
    public class FlagEntryDto
    {
        public string id = "";
        public bool value = false;
    }
}
