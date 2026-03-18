using Template.Emotes;
using ServiceLocator = Template.Services.Services;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Template.Achievements.SignalReporters
{
    /// <summary>
    /// Listens for successful emote plays, updates save-backed progression, and
    /// publishes the generic achievement signal for emote-based achievements.
    /// </summary>
    [MovedFrom(true, null, "Assembly-CSharp", "EmoteAchievementSignalReporter")]
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
            var playerData = ServiceLocator.SaveService?.GameDataCache?.Player;
            if (playerData == null)
            {
                return;
            }

            playerData.TotalEmotesPerformed += 1;
            ServiceLocator.SaveService?.MarkGameDirty();
            AchievementSignalBus.Publish(AchievementSignalKeys.EmotePerformed);
        }
    }
}
