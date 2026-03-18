using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Unlocks when a specific scene has been visited.
/// </summary>
[CreateAssetMenu(menuName = "Achievements/Conditions/Scene Visited", fileName = "SceneVisitedCondition")]
public class SceneVisitedCondition : AchievementUnlockCondition
{
    private static readonly string[] SignalKeys =
    {
        AchievementSignalKeys.SceneVisited
    };

    [SerializeField] private string requiredSceneName;

    public override IReadOnlyList<string> RelevantSignalKeys => SignalKeys;

    public override AchievementConditionEvaluationResult Evaluate(
        AchievementEvaluationContext evaluationContext,
        AchievementProgressState progressState)
    {
        var isUnlocked = evaluationContext.HasVisitedScene(requiredSceneName);
        var progressValue = isUnlocked ? 1 : 0;
        return new AchievementConditionEvaluationResult(isUnlocked, progressValue, 1);
    }
}