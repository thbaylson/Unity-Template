using UnityEngine;

/// <summary>
/// Base unlock condition that evaluates one achievement against current player context.
/// </summary>
public abstract class AchievementUnlockCondition : ScriptableObject
{
    public abstract AchievementConditionEvaluationResult Evaluate(AchievementEvaluationContext evaluationContext, AchievementProgressState progressState);
}
