using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base unlock condition that evaluates one achievement against current player context.
/// Each condition declares which progression signals should cause re-evaluation.
/// </summary>
public abstract class AchievementUnlockCondition : ScriptableObject
{
    public abstract IReadOnlyList<string> RelevantSignalKeys { get; }

    public abstract AchievementConditionEvaluationResult Evaluate(
        AchievementEvaluationContext evaluationContext,
        AchievementProgressState progressState);
}