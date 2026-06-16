using System;
using System.Collections.Generic;
using UnityEngine;

namespace Template.BetterInputHandling
{
    /// <summary>
    /// Data asset that maps normalized control keys to project-specific glyph sprites and text fallbacks.
    /// </summary>
    [CreateAssetMenu(menuName = "Better Input Handling/Glyph Set", fileName = "BetterInputGlyphSet")]
    public class BetterInputGlyphSet : ScriptableObject
    {
        [SerializeField] private BetterInputDeviceKind deviceKind = BetterInputDeviceKind.KeyboardMouse;
        [SerializeField] private string displayName = "Glyph Set";
        [SerializeField] private List<BetterInputGlyphEntry> entries = new List<BetterInputGlyphEntry>();

        public BetterInputDeviceKind DeviceKind => deviceKind;
        public string DisplayName => displayName;

        public bool TryGetGlyph(string controlPathOrKey, out BetterInputResolvedGlyph glyph)
        {
            var controlKey = BetterInputControlPathUtility.NormalizeControlPath(controlPathOrKey);
            foreach (var entry in entries)
            {
                if (entry == null || !entry.Matches(controlKey))
                {
                    continue;
                }

                glyph = new BetterInputResolvedGlyph(entry.Sprite, entry.TextFallback, controlKey);
                return true;
            }

            glyph = default;
            return false;
        }

        public void Configure(BetterInputDeviceKind kind, string setDisplayName, IEnumerable<BetterInputGlyphEntry> glyphEntries)
        {
            deviceKind = kind;
            displayName = setDisplayName;
            entries = glyphEntries != null ? new List<BetterInputGlyphEntry>(glyphEntries) : new List<BetterInputGlyphEntry>();
        }
    }

    /// <summary>
    /// Single glyph mapping entry inside a BetterInputGlyphSet.
    /// </summary>
    [Serializable]
    public class BetterInputGlyphEntry
    {
        public BetterInputGlyphEntry(string controlKey, Sprite sprite, string textFallback)
        {
            this.controlKey = BetterInputControlPathUtility.NormalizeControlPath(controlKey);
            this.sprite = sprite;
            this.textFallback = textFallback;
        }

        public string ControlKey => controlKey;
        public Sprite Sprite => sprite;
        public string TextFallback => string.IsNullOrWhiteSpace(textFallback)
            ? BetterInputControlPathUtility.ToDisplayName(controlKey)
            : textFallback;

        public bool Matches(string candidateControlKey)
        {
            return string.Equals(controlKey, candidateControlKey, StringComparison.OrdinalIgnoreCase);
        }

        [SerializeField] private string controlKey;
        [SerializeField] private Sprite sprite;
        [SerializeField] private string textFallback;
    }

    /// <summary>
    /// Runtime result produced when an input control has been resolved to a visual glyph.
    /// </summary>
    public readonly struct BetterInputResolvedGlyph
    {
        public BetterInputResolvedGlyph(Sprite sprite, string textFallback, string controlKey)
        {
            Sprite = sprite;
            TextFallback = textFallback;
            ControlKey = controlKey;
        }

        public Sprite Sprite { get; }
        public string TextFallback { get; }
        public string ControlKey { get; }
        public bool HasSprite => Sprite != null;
    }
}
