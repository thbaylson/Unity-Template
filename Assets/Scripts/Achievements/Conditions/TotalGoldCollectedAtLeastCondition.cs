using UnityEngine;

/// <summary>
/// Unlocks when the player's lifetime collected gold reaches a required amount.
/// </summary>
[CreateAssetMenu(menuName = "Achievements/Conditions/Total Gold Collected At Least", fileName = "TotalGoldCollectedAtLeastCondition")]
public class TotalGoldCollectedAtLeastCondition : AchievementUnlockCondition
{
    [SerializeField] private int requiredCollectedGoldAmount = 1;

    public override AchievementConditionEvaluationResult Evaluate(AchievementEvaluationContext evaluationContext, AchievementProgressState progressState)
    {
        var clampedProgress = Mathf.Clamp(evaluationContext.TotalGoldCollected, 0, requiredCollectedGoldAmount);
        var isUnlocked = evaluationContext.TotalGoldCollected >= requiredCollectedGoldAmount;
        return new AchievementConditionEvaluationResult(isUnlocked, clampedProgress, requiredCollectedGoldAmount);
    }
}
