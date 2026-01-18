using System.Collections.Generic;

public class GameDataCache
{
    public PlayerSaveData Player = new PlayerSaveData();

    // sceneName -> (interactableId -> flag)
    public readonly Dictionary<string, Dictionary<string, bool>> LevelInteractableFlags = new Dictionary<string, Dictionary<string, bool>>();
}
