public readonly struct AchievementEvaluationContext
{
    public AchievementEvaluationContext(int currentGoldOwned, int totalGoldCollected, string mostRecentScene)
    {
        CurrentGoldOwned = currentGoldOwned;
        TotalGoldCollected = totalGoldCollected;
        MostRecentScene = mostRecentScene;
    }

    public int CurrentGoldOwned { get; }
    public int TotalGoldCollected { get; }
    public string MostRecentScene { get; }
}
