using System;

namespace Template.Achievements
{
    [Serializable]
    public class AchievementProgressState
    {
        public string AchievementId = string.Empty;
        public bool IsUnlocked = false;
        public long UnlockedUnixTime = 0;
        public int CurrentProgressValue = 0;
    }
}
