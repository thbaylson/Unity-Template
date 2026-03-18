using System.Collections.Generic;
using System.Linq;

public readonly struct AchievementEvaluationContext
{
    public AchievementEvaluationContext(
        int currentGoldOwned,
        int totalGoldCollected,
        IReadOnlyCollection<string> visitedSceneNames,
        string mostRecentScene)
    {
        CurrentGoldOwned = currentGoldOwned;
        TotalGoldCollected = totalGoldCollected;
        VisitedSceneNames = visitedSceneNames;
        MostRecentScene = mostRecentScene;
    }

    public int CurrentGoldOwned { get; }
    public int TotalGoldCollected { get; }
    public IReadOnlyCollection<string> VisitedSceneNames { get; }
    public string MostRecentScene { get; }

    public bool HasVisitedScene(string sceneName)
    {
        if (string.IsNullOrWhiteSpace(sceneName) || VisitedSceneNames == null) return false;

        return VisitedSceneNames.Contains(sceneName);
    }
}