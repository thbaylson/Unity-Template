using NUnit.Framework;
using Template.BetterInputHandling;

/// <summary>
/// Verifies BetterInputHandling control path normalization and binding group matching.
/// </summary>
public class BetterInputControlPathUtilityTests
{
    [Test]
    public void NormalizeControlPath_ReturnsLastControlSegment()
    {
        Assert.That(BetterInputControlPathUtility.NormalizeControlPath("<Keyboard>/escape"), Is.EqualTo("esc"));
        Assert.That(BetterInputControlPathUtility.NormalizeControlPath("<Gamepad>/buttonSouth"), Is.EqualTo("buttonSouth"));
    }

    [Test]
    public void BindingMatchesAnyGroup_TreatsKeyboardMouseVariantsAsEquivalent()
    {
        var groups = new[] { "KeyboardMouse", "Keyboard&Mouse" };

        Assert.That(BetterInputControlPathUtility.BindingMatchesAnyGroup(";KeyboardMouse", groups), Is.True);
        Assert.That(BetterInputControlPathUtility.BindingMatchesAnyGroup("Keyboard&Mouse", groups), Is.True);
    }

    [Test]
    public void BindingMatchesAnyGroup_MatchesSwitchControllerAliases()
    {
        var groups = new[] { "Switch Controller", "Gamepad" };

        Assert.That(BetterInputControlPathUtility.BindingMatchesAnyGroup("Switch Controller;Gamepad", groups), Is.True);
    }

    [Test]
    public void ToDisplayName_ProvidesReadableFallbacks()
    {
        Assert.That(BetterInputControlPathUtility.ToDisplayName("<Keyboard>/escape"), Is.EqualTo("Esc"));
        Assert.That(BetterInputControlPathUtility.ToDisplayName("<Gamepad>/buttonNorth"), Is.EqualTo("Y"));
        Assert.That(BetterInputControlPathUtility.ToDisplayName("<Keyboard>/e"), Is.EqualTo("E"));
    }

    [Test]
    public void DeviceProfile_RecognizesPlayStationControlScheme()
    {
        var profile = BetterInputDeviceProfile.FromControlScheme("PS4 Controller", null);

        Assert.That(profile.Kind, Is.EqualTo(BetterInputDeviceKind.PlayStationGamepad));
        Assert.That(profile.IsGamepad, Is.True);
    }

    [Test]
    public void DeviceProfile_RecognizesSwitchControlScheme()
    {
        var profile = BetterInputDeviceProfile.FromControlScheme("Switch Controller", null);

        Assert.That(profile.Kind, Is.EqualTo(BetterInputDeviceKind.GenericGamepad));
        Assert.That(profile.PrimaryBindingGroup, Is.EqualTo("Switch Controller"));
        Assert.That(profile.IsGamepad, Is.True);
    }
}
