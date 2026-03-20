using System.Collections.Generic;

namespace Template.Services.Saving
{
    public class GameDataCache
    {
        public PlayerSaveData Player = new PlayerSaveData();

        // sceneName -> (interactableId -> flag)
        public readonly Dictionary<string, Dictionary<string, bool>> LevelInteractableFlags = new Dictionary<string, Dictionary<string, bool>>();
    }
}
