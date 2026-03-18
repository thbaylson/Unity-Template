using NUnit.Framework;
using Template.Achievements;
using Template.UI;

/// <summary>
/// Verifies achievement menu progress formatting without requiring scene or UI setup.
/// </summary>
public class AchievementProgressTextFormatterTests
{
    [Test]
    public void BuildDescriptionText_AppendsProgressLine_ForLockedProgressiveAchievements()
    {
        var description = AchievementProgressTextFormatter.BuildDescriptionText(
            "Collect 10 G.",
            new AchievementDisplayProgress(false, 3, 10));

        Assert.That(description, Is.EqualTo("Collect 10 G.\n<size=24><color=#BFBFBF>Progress: 3 / 10</color></size>"));
    }

    [Test]
    public void BuildDescriptionText_DoesNotAppendProgressLine_ForBinaryAchievements()
    {
        var description = AchievementProgressTextFormatter.BuildDescriptionText(
            "Visit the PBInterior Level.",
            new AchievementDisplayProgress(false, 0, 1));

        Assert.That(description, Is.EqualTo("Visit the PBInterior Level."));
    }

    [Test]
    public void BuildDescriptionText_DoesNotAppendProgressLine_WhenAchievementIsUnlocked()
    {
        var description = AchievementProgressTextFormatter.BuildDescriptionText(
            "Collect 10 G.",
            new AchievementDisplayProgress(true, 10, 10));

        Assert.That(description, Is.EqualTo("Collect 10 G."));
    }
}
