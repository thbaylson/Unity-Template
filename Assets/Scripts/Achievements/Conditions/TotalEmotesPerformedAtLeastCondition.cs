using System.Collections.Generic;
using Template.Achievements;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Template.Achievements.Conditions
{
    /// <summary>
    /// Unlocks when the player has successfully started a required number of emotes.
    /// </summary>
    [MovedFrom(true, null, "Assembly-CSharp", "TotalEmotesPerformedAtLeastCondition")]
    [CreateAssetMenu(menuName = "Achievements/Conditions/Total Emotes Performed At Least", fileName = "TotalEmotesPerformedAtLeastCondition")]
    public class TotalEmotesPerformedAtLeastCondition : AchievementUnlockCondition
    {
        private static readonly string[] SignalKeys =
        {
            AchievementSignalKeys.EmotePerformed
        };

        [SerializeField] private int requiredEmoteCount = 1;

        public override IReadOnlyList<string> RelevantSignalKeys => SignalKeys;

        public override AchievementConditionEvaluationResult Evaluate(
            AchievementEvaluationContext evaluationContext,
            AchievementProgressState progressState)
        {
            var clampedProgress = Mathf.Clamp(
                evaluationContext.TotalEmotesPerformed,
                0,
                requiredEmoteCount);

            var isUnlocked = evaluationContext.TotalEmotesPerformed >= requiredEmoteCount;

            return new AchievementConditionEvaluationResult(
                isUnlocked,
                clampedProgress,
                requiredEmoteCount);
        }
    }
}
