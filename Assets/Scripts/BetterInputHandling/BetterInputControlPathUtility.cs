using System;
using System.Collections.Generic;
using System.Linq;

namespace Template.BetterInputHandling
{
    /// <summary>
    /// Provides stable normalization helpers for Unity Input System control paths and binding groups.
    /// </summary>
    public static class BetterInputControlPathUtility
    {
        private static readonly Dictionary<string, string> DisplayNames = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["escape"] = "Esc",
            ["esc"] = "Esc",
            ["space"] = "Space",
            ["spacebar"] = "Space",
            ["leftShift"] = "Shift",
            ["rightShift"] = "Shift",
            ["shift"] = "Shift",
            ["enter"] = "Enter",
            ["return"] = "Enter",
            ["leftButton"] = "LMB",
            ["rightButton"] = "RMB",
            ["middleButton"] = "MMB",
            ["buttonSouth"] = "A",
            ["buttonEast"] = "B",
            ["buttonWest"] = "X",
            ["buttonNorth"] = "Y",
            ["start"] = "Start",
            ["select"] = "Select",
            ["leftShoulder"] = "LB",
            ["rightShoulder"] = "RB",
            ["leftTrigger"] = "LT",
            ["rightTrigger"] = "RT",
        };

        private static readonly Dictionary<string, string> SpriteKeys = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["escape"] = "esc",
            ["esc"] = "esc",
            ["space"] = "spacebar",
            ["spacebar"] = "spacebar",
            ["leftShift"] = "shift",
            ["rightShift"] = "shift",
            ["shift"] = "shift",
            ["digit1"] = "number_1",
            ["1"] = "number_1",
            ["digit2"] = "number_2",
            ["2"] = "number_2",
            ["leftShoulder"] = "leftShoulder",
            ["rightShoulder"] = "rightShoulder",
        };

        public static string NormalizeBindingGroup(string bindingGroup)
        {
            if (string.IsNullOrWhiteSpace(bindingGroup))
            {
                return string.Empty;
            }

            return new string(bindingGroup
                .Where(c => !char.IsWhiteSpace(c) && c != '&' && c != '-' && c != '_')
                .Select(char.ToLowerInvariant)
                .ToArray());
        }

        public static string NormalizeControlPath(string controlPath)
        {
            if (string.IsNullOrWhiteSpace(controlPath))
            {
                return string.Empty;
            }

            var path = controlPath.Trim();
            var braceIndex = path.LastIndexOf("/{", StringComparison.Ordinal);
            if (braceIndex >= 0 && path.EndsWith("}", StringComparison.Ordinal))
            {
                return path.Substring(braceIndex + 2).Trim('{', '}');
            }

            var slashIndex = path.LastIndexOf('/');
            var key = slashIndex >= 0 ? path.Substring(slashIndex + 1) : path;
            key = key.Trim('{', '}');

            return SpriteKeys.TryGetValue(key, out var spriteKey) ? spriteKey : key;
        }

        public static string ToDisplayName(string controlPathOrKey)
        {
            var key = NormalizeControlPath(controlPathOrKey);
            if (string.IsNullOrWhiteSpace(key))
            {
                return string.Empty;
            }

            if (DisplayNames.TryGetValue(key, out var displayName))
            {
                return displayName;
            }

            if (key.Length == 1)
            {
                return key.ToUpperInvariant();
            }

            return key;
        }

        public static bool BindingMatchesAnyGroup(string bindingGroups, IReadOnlyList<string> candidateGroups)
        {
            if (candidateGroups == null || candidateGroups.Count == 0)
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(bindingGroups))
            {
                return false;
            }

            var normalizedBindingGroups = bindingGroups
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(NormalizeBindingGroup)
                .Where(group => !string.IsNullOrWhiteSpace(group))
                .ToArray();

            foreach (var candidateGroup in candidateGroups)
            {
                var normalizedCandidate = NormalizeBindingGroup(candidateGroup);
                if (string.IsNullOrWhiteSpace(normalizedCandidate))
                {
                    continue;
                }

                if (normalizedBindingGroups.Any(group => group == normalizedCandidate))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
