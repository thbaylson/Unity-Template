public readonly struct AchievementDisplayProgress
{
    public AchievementDisplayProgress(bool isUnlocked, int currentProgressValue, int targetProgressValue)
    {
        IsUnlocked = isUnlocked;
        CurrentProgressValue = currentProgressValue;
        TargetProgressValue = targetProgressValue;
    }

    public bool IsUnlocked { get; }
    public int CurrentProgressValue { get; }
    public int TargetProgressValue { get; }
}
