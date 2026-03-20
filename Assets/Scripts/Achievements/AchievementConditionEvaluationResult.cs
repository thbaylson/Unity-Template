namespace Template.Achievements
{
    public readonly struct AchievementConditionEvaluationResult
    {
        public AchievementConditionEvaluationResult(bool isUnlocked, int progressValue, int unlockProgressValue)
        {
            IsUnlocked = isUnlocked;
            ProgressValue = progressValue;
            UnlockProgressValue = unlockProgressValue;
        }

        public bool IsUnlocked { get; }
        public int ProgressValue { get; }
        public int UnlockProgressValue { get; }
    }
}
