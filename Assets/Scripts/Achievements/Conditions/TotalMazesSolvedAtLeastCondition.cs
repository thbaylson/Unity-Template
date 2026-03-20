using System.Collections.Generic;
using Template.Achievements;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Template.Achievements.Conditions
{
    /// <summary>
    /// Unlocks when the player has completed a minimum number of mazes.
    /// </summary>
    [MovedFrom(true, null, "Assembly-CSharp", "TotalMazesSolvedAtLeastCondition")]
    [CreateAssetMenu(menuName = "Achievements/Conditions/Total Mazes Solved At Least", fileName = "TotalMazesSolvedAtLeastCondition")]
    public class TotalMazesSolvedAtLeastCondition : AchievementUnlockCondition
    {
        private static readonly string[] SignalKeys =
        {
            AchievementSignalKeys.MazeCompleted
        };

        [SerializeField] private int requiredSolvedMazeCount = 1;

        public override IReadOnlyList<string> RelevantSignalKeys => SignalKeys;

        public override AchievementConditionEvaluationResult Evaluate(
            AchievementEvaluationContext evaluationContext,
            AchievementProgressState progressState)
        {
            var unlockCount = Mathf.Max(1, requiredSolvedMazeCount);
            var progressValue = Mathf.Clamp(evaluationContext.TotalMazesSolved, 0, unlockCount);
            var isUnlocked = evaluationContext.TotalMazesSolved >= unlockCount;

            return new AchievementConditionEvaluationResult(isUnlocked, progressValue, unlockCount);
        }
    }
}
