using NUnit.Framework;
using Template.BetterInputHandling;
using UnityEngine;

/// <summary>
/// Verifies BetterInputHandling glyph set lookup behavior.
/// </summary>
public class BetterInputGlyphSetTests
{
    [Test]
    public void TryGetGlyph_ReturnsTextFallback_WhenSpriteIsMissing()
    {
        var glyphSet = ScriptableObject.CreateInstance<BetterInputGlyphSet>();
        glyphSet.Configure(
            BetterInputDeviceKind.KeyboardMouse,
            "Keyboard",
            new[] { new BetterInputGlyphEntry("escape", null, "Esc") });

        var found = glyphSet.TryGetGlyph("<Keyboard>/escape", out var glyph);

        Assert.That(found, Is.True);
        Assert.That(glyph.TextFallback, Is.EqualTo("Esc"));
        Assert.That(glyph.HasSprite, Is.False);
    }

    [Test]
    public void ActionReference_UsesMapAndActionForEquality()
    {
        var first = new BetterInputActionReference("Player", "Interact");
        var second = new BetterInputActionReference("Player", "Interact");
        var different = new BetterInputActionReference("UI", "Submit");

        Assert.That(first.Equals(second), Is.True);
        Assert.That(first.Equals(different), Is.False);
    }
}
