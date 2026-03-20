using System.Collections.Generic;
using Template.Achievements;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Template.Achievements.Conditions
{
    /// <summary>
    /// Unlocks when the player's lifetime collected gold reaches a required amount.
    /// </summary>
    [MovedFrom(true, null, "Assembly-CSharp", "TotalGoldCollectedAtLeastCondition")]
    [CreateAssetMenu(menuName = "Achievements/Conditions/Total Gold Collected At Least", fileName = "TotalGoldCollectedAtLeastCondition")]
    public class TotalGoldCollectedAtLeastCondition : AchievementUnlockCondition
    {
        private static readonly string[] SignalKeys =
        {
            AchievementSignalKeys.GoldCollected
        };

        [SerializeField] private int requiredCollectedGoldAmount = 1;

        public override IReadOnlyList<string> RelevantSignalKeys => SignalKeys;

        public override AchievementConditionEvaluationResult Evaluate(
            AchievementEvaluationContext evaluationContext,
            AchievementProgressState progressState)
        {
            var clampedProgress = Mathf.Clamp(
                evaluationContext.TotalGoldCollected,
                0,
                requiredCollectedGoldAmount);

            var isUnlocked = evaluationContext.TotalGoldCollected >= requiredCollectedGoldAmount;

            return new AchievementConditionEvaluationResult(
                isUnlocked,
                clampedProgress,
                requiredCollectedGoldAmount);
        }
    }
}
