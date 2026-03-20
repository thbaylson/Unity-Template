using System.Collections.Generic;
using Template.Achievements;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Template.Achievements.Conditions
{
    /// <summary>
    /// Base unlock condition that evaluates one achievement against current player context.
    /// Each condition declares which progression signals should cause re-evaluation.
    /// </summary>
    [MovedFrom(true, null, "Assembly-CSharp", "AchievementUnlockCondition")]
    public abstract class AchievementUnlockCondition : ScriptableObject
    {
        public abstract IReadOnlyList<string> RelevantSignalKeys { get; }

        public abstract AchievementConditionEvaluationResult Evaluate(
            AchievementEvaluationContext evaluationContext,
            AchievementProgressState progressState);
    }
}
