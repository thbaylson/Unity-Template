using System.Reflection;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Verifies built-in achievement unlock conditions in isolation so unlock
/// evaluation can be covered with fast EditMode tests.
/// </summary>
public class AchievementUnlockEvaluatorTests
{
    [Test]
    public void TotalGoldCollectedCondition_LocksBelowThreshold()
    {
        var condition = CreateCondition<TotalGoldCollectedAtLeastCondition>(
            "requiredCollectedGoldAmount",
            10);

        try
        {
            var result = condition.Evaluate(
                CreateContext(totalGoldCollected: 4),
                new AchievementProgressState());

            Assert.That(result.IsUnlocked, Is.False);
            Assert.That(result.ProgressValue, Is.EqualTo(4));
            Assert.That(result.UnlockProgressValue, Is.EqualTo(10));
            Assert.That(condition.RelevantSignalKeys, Is.EqualTo(new[] { AchievementSignalKeys.GoldCollected }));
        }
        finally
        {
            Object.DestroyImmediate(condition);
        }
    }

    [Test]
    public void TotalGoldCollectedCondition_UnlocksAndClampsAtThreshold()
    {
        var condition = CreateCondition<TotalGoldCollectedAtLeastCondition>(
            "requiredCollectedGoldAmount",
            10);

        try
        {
            var result = condition.Evaluate(
                CreateContext(totalGoldCollected: 25),
                new AchievementProgressState());

            Assert.That(result.IsUnlocked, Is.True);
            Assert.That(result.ProgressValue, Is.EqualTo(10));
            Assert.That(result.UnlockProgressValue, Is.EqualTo(10));
        }
        finally
        {
            Object.DestroyImmediate(condition);
        }
    }

    [Test]
    public void TotalGoldOwnedCondition_UsesCurrentOwnedGold()
    {
        var condition = CreateCondition<TotalGoldOwnedAtLeastCondition>(
            "requiredGoldAmount",
            7);

        try
        {
            var result = condition.Evaluate(
                CreateContext(currentGoldOwned: 7, totalGoldCollected: 20),
                new AchievementProgressState());

            Assert.That(result.IsUnlocked, Is.True);
            Assert.That(result.ProgressValue, Is.EqualTo(7));
            Assert.That(result.UnlockProgressValue, Is.EqualTo(7));
            Assert.That(condition.RelevantSignalKeys, Is.EqualTo(new[] { AchievementSignalKeys.GoldOwnedChanged }));
        }
        finally
        {
            Object.DestroyImmediate(condition);
        }
    }

    [Test]
    public void SceneVisitedCondition_UnlocksOnlyWhenRequiredSceneHasBeenVisited()
    {
        var condition = CreateCondition<SceneVisitedCondition>(
            "requiredSceneName",
            "ProBuilderInterior");

        try
        {
            var lockedResult = condition.Evaluate(
                CreateContext(visitedSceneNames: new[] { "Title", "FlatScene" }),
                new AchievementProgressState());

            var unlockedResult = condition.Evaluate(
                CreateContext(visitedSceneNames: new[] { "Title", "ProBuilderInterior" }),
                new AchievementProgressState());

            Assert.That(lockedResult.IsUnlocked, Is.False);
            Assert.That(lockedResult.ProgressValue, Is.EqualTo(0));
            Assert.That(unlockedResult.IsUnlocked, Is.True);
            Assert.That(unlockedResult.ProgressValue, Is.EqualTo(1));
            Assert.That(unlockedResult.UnlockProgressValue, Is.EqualTo(1));
            Assert.That(condition.RelevantSignalKeys, Is.EqualTo(new[] { AchievementSignalKeys.SceneVisited }));
        }
        finally
        {
            Object.DestroyImmediate(condition);
        }
    }

    private static AchievementEvaluationContext CreateContext(
        int currentGoldOwned = 0,
        int totalGoldCollected = 0,
        string[] visitedSceneNames = null)
    {
        if (visitedSceneNames == null)
        {
            visitedSceneNames = new string[0];
        }

        var mostRecentScene = visitedSceneNames.Length > 0
            ? visitedSceneNames[visitedSceneNames.Length - 1]
            : string.Empty;

        return new AchievementEvaluationContext(
            currentGoldOwned,
            totalGoldCollected,
            visitedSceneNames,
            mostRecentScene);
    }

    private static TCondition CreateCondition<TCondition>(string fieldName, object value)
        where TCondition : ScriptableObject
    {
        var condition = ScriptableObject.CreateInstance<TCondition>();
        var field = typeof(TCondition).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.That(field, Is.Not.Null, $"{typeof(TCondition).Name} is missing field '{fieldName}'.");
        field.SetValue(condition, value);

        return condition;
    }
}
