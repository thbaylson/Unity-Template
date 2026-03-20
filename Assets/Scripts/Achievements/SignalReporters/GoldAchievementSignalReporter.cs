using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Template.Achievements.SignalReporters
{
    /// <summary>
    /// Listens to gold-related gameplay events and translates them into generic achievement
    /// signals so gold-based achievements can be evaluated without coupling the gold system
    /// directly to the achievement service.
    /// </summary>
    [MovedFrom(true, null, "Assembly-CSharp", "GoldAchievementSignalReporter")]
    public class GoldAchievementSignalReporter : MonoBehaviour
    {
        private void OnEnable()
        {
            GoldCollector.OnGoldCollected += OnGoldCollected;
            GoldCollector.OnGoldChanged += OnGoldChanged;
        }

        private void OnDisable()
        {
            GoldCollector.OnGoldCollected -= OnGoldCollected;
            GoldCollector.OnGoldChanged -= OnGoldChanged;
        }

        private static void OnGoldCollected(int amountCollected, int currentGoldOwned)
        {
            if (amountCollected <= 0) return;

            AchievementSignalBus.Publish(AchievementSignalKeys.GoldCollected);
        }

        private static void OnGoldChanged(int currentGoldOwned)
        {
            AchievementSignalBus.Publish(AchievementSignalKeys.GoldOwnedChanged);
        }
    }
}
