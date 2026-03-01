using System;
using UnityEngine;

/// <summary>
/// Unlocks when a specific scene has been visited.
/// </summary>
[CreateAssetMenu(menuName = "Achievements/Conditions/Scene Visited", fileName = "SceneVisitedCondition")]
public class SceneVisitedCondition : AchievementUnlockCondition
{
    [SerializeField] private string requiredSceneName;

    public override AchievementConditionEvaluationResult Evaluate(AchievementEvaluationContext evaluationContext, AchievementProgressState progressState)
    {
        var isUnlocked = string.Equals(requiredSceneName, evaluationContext.MostRecentScene, StringComparison.Ordinal);
        var progressValue = isUnlocked ? 1 : 0;
        return new AchievementConditionEvaluationResult(isUnlocked, progressValue, 1);
    }
}
