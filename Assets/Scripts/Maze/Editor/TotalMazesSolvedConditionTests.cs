using System.Reflection;
using NUnit.Framework;
using Template.Achievements;
using Template.Achievements.Conditions;
using UnityEngine;

/// <summary>
/// Verifies maze-completion achievement conditions without requiring scene setup.
/// </summary>
public class TotalMazesSolvedConditionTests
{
    [Test]
    public void Evaluate_UnlocksOnlyAfterRequiredMazeCount()
    {
        var condition = ScriptableObject.CreateInstance<TotalMazesSolvedAtLeastCondition>();
        var field = typeof(TotalMazesSolvedAtLeastCondition).GetField(
            "requiredSolvedMazeCount",
            BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(field, Is.Not.Null);
        field.SetValue(condition, 2);

        try
        {
            var lockedResult = condition.Evaluate(
                new AchievementEvaluationContext(0, 0, 0, 1, new string[0], string.Empty),
                new AchievementProgressState());
            var unlockedResult = condition.Evaluate(
                new AchievementEvaluationContext(0, 0, 0, 2, new string[0], string.Empty),
                new AchievementProgressState());

            Assert.That(lockedResult.IsUnlocked, Is.False);
            Assert.That(lockedResult.ProgressValue, Is.EqualTo(1));
            Assert.That(lockedResult.UnlockProgressValue, Is.EqualTo(2));
            Assert.That(unlockedResult.IsUnlocked, Is.True);
            Assert.That(unlockedResult.ProgressValue, Is.EqualTo(2));
            Assert.That(unlockedResult.UnlockProgressValue, Is.EqualTo(2));
            Assert.That(condition.RelevantSignalKeys, Is.EqualTo(new[] { AchievementSignalKeys.MazeCompleted }));
        }
        finally
        {
            Object.DestroyImmediate(condition);
        }
    }
}
