using System;

namespace Template.Achievements
{
    /// <summary>
    /// Publishes lightweight achievement-related progression signals so gameplay systems
    /// and reporter components can notify the achievement system.
    /// </summary>
    public static class AchievementSignalBus
    {
        public static event Action<string> SignalRaised;

        public static void Publish(string signalKey)
        {
            if (string.IsNullOrWhiteSpace(signalKey)) return;

            SignalRaised?.Invoke(signalKey);
        }
    }
}
