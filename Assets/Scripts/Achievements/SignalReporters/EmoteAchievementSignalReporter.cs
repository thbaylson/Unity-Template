using Template.Emotes;
using UnityEngine;

/// <summary>
/// Listens for successful emote plays, updates save-backed progression, and
/// publishes the generic achievement signal for emote-based achievements.
/// </summary>
public class EmoteAchievementSignalReporter : MonoBehaviour
{
    private void OnEnable()
    {
        EmoteController.EmotePlayed += OnEmotePlayed;
    }

    private void OnDisable()
    {
        EmoteController.EmotePlayed -= OnEmotePlayed;
    }

    private static void OnEmotePlayed(EmoteDefinition emote)
    {
        var playerData = Services.SaveService?.GameDataCache?.Player;
        if (playerData == null)
        {
            return;
        }

        playerData.TotalEmotesPerformed += 1;
        Services.SaveService?.MarkGameDirty();
        AchievementSignalBus.Publish(AchievementSignalKeys.EmotePerformed);
    }
}
