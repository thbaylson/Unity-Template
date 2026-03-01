using UnityEngine;

/// <summary>
/// Unlocks when the player's currently owned gold reaches a required amount.
/// </summary>
[CreateAssetMenu(menuName = "Achievements/Conditions/Total Gold Owned At Least", fileName = "TotalGoldOwnedAtLeastCondition")]
public class TotalGoldOwnedAtLeastCondition : AchievementUnlockCondition
{
    [SerializeField] private int requiredGoldAmount = 1;

    public override AchievementConditionEvaluationResult Evaluate(AchievementEvaluationContext evaluationContext, AchievementProgressState progressState)
    {
        var clampedProgress = Mathf.Clamp(evaluationContext.CurrentGoldOwned, 0, requiredGoldAmount);
        var isUnlocked = evaluationContext.CurrentGoldOwned >= requiredGoldAmount;
        return new AchievementConditionEvaluationResult(isUnlocked, clampedProgress, requiredGoldAmount);
    }
}
