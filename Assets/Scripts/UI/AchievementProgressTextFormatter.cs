public static class AchievementProgressTextFormatter
{
    private const string ProgressTextColor = "#BFBFBF";

    public static string BuildDescriptionText(string description, AchievementDisplayProgress progress)
    {
        var safeDescription = description ?? string.Empty;

        if (progress.IsUnlocked || progress.TargetProgressValue <= 1)
        {
            return safeDescription;
        }

        var progressLine =
            $"<size=24><color={ProgressTextColor}>Progress: {progress.CurrentProgressValue} / {progress.TargetProgressValue}</color></size>";

        if (string.IsNullOrWhiteSpace(safeDescription))
        {
            return progressLine;
        }

        return $"{safeDescription}\n{progressLine}";
    }
}
