using System.Collections.Generic;
using Template.Achievements;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Template.Achievements.Conditions
{
    /// <summary>
    /// Unlocks when the player's currently owned gold reaches a required amount.
    /// </summary>
    [MovedFrom(true, null, "Assembly-CSharp", "TotalGoldOwnedAtLeastCondition")]
    [CreateAssetMenu(menuName = "Achievements/Conditions/Total Gold Owned At Least", fileName = "TotalGoldOwnedAtLeastCondition")]
    public class TotalGoldOwnedAtLeastCondition : AchievementUnlockCondition
    {
        private static readonly string[] SignalKeys =
        {
            AchievementSignalKeys.GoldOwnedChanged
        };

        [SerializeField] private int requiredGoldAmount = 1;

        public override IReadOnlyList<string> RelevantSignalKeys => SignalKeys;

        public override AchievementConditionEvaluationResult Evaluate(
            AchievementEvaluationContext evaluationContext,
            AchievementProgressState progressState)
        {
            var clampedProgress = Mathf.Clamp(
                evaluationContext.CurrentGoldOwned,
                0,
                requiredGoldAmount);

            var isUnlocked = evaluationContext.CurrentGoldOwned >= requiredGoldAmount;

            return new AchievementConditionEvaluationResult(
                isUnlocked,
                clampedProgress,
                requiredGoldAmount);
        }
    }
}
