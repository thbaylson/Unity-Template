using System.Collections.Generic;
using Template.Achievements;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Template.Achievements.Conditions
{
    /// <summary>
    /// Unlocks when a specific scene has been visited.
    /// </summary>
    [MovedFrom(true, null, "Assembly-CSharp", "SceneVisitedCondition")]
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
}
