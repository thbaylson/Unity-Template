using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Template.BetterInputHandling
{
    /// <summary>
    /// Project-level configuration for BetterInputHandling action assets, glyph sets, and remappable actions.
    /// </summary>
    [CreateAssetMenu(menuName = "Better Input Handling/Settings", fileName = "BetterInputSettings")]
    public class BetterInputSettings : ScriptableObject
    {
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField] private string gameplayActionMap = "Player";
        [SerializeField] private string uiActionMap = "UI";
        [SerializeField] private string bindingOverridesPlayerPrefsKey = "betterInput.bindingOverrides";
        [SerializeField] private BetterInputGlyphSet keyboardMouseGlyphSet;
        [SerializeField] private BetterInputGlyphSet xboxGlyphSet;
        [SerializeField] private BetterInputGlyphSet playStationGlyphSet;
        [SerializeField] private BetterInputGlyphSet genericGamepadGlyphSet;
        [SerializeField] private List<BetterInputRemappableAction> remappableActions = new List<BetterInputRemappableAction>();

        public InputActionAsset InputActions => inputActions;
        public string GameplayActionMap => gameplayActionMap;
        public string UIActionMap => uiActionMap;
        public string BindingOverridesPlayerPrefsKey => bindingOverridesPlayerPrefsKey;
        public IReadOnlyList<BetterInputRemappableAction> RemappableActions => remappableActions;

        public BetterInputGlyphSet GetGlyphSet(BetterInputDeviceKind deviceKind)
        {
            return deviceKind switch
            {
                BetterInputDeviceKind.KeyboardMouse => keyboardMouseGlyphSet,
                BetterInputDeviceKind.XboxGamepad => xboxGlyphSet != null ? xboxGlyphSet : genericGamepadGlyphSet,
                BetterInputDeviceKind.PlayStationGamepad => playStationGlyphSet != null ? playStationGlyphSet : genericGamepadGlyphSet,
                BetterInputDeviceKind.GenericGamepad => genericGamepadGlyphSet != null ? genericGamepadGlyphSet : xboxGlyphSet,
                _ => keyboardMouseGlyphSet,
            };
        }

        public void Configure(
            InputActionAsset actions,
            BetterInputGlyphSet keyboardGlyphs,
            BetterInputGlyphSet xboxGlyphs,
            BetterInputGlyphSet playStationGlyphs,
            BetterInputGlyphSet genericGlyphs,
            IEnumerable<BetterInputRemappableAction> actionsToRebind)
        {
            inputActions = actions;
            keyboardMouseGlyphSet = keyboardGlyphs;
            xboxGlyphSet = xboxGlyphs;
            playStationGlyphSet = playStationGlyphs;
            genericGamepadGlyphSet = genericGlyphs;
            remappableActions = actionsToRebind != null
                ? new List<BetterInputRemappableAction>(actionsToRebind)
                : new List<BetterInputRemappableAction>();
        }
    }

    /// <summary>
    /// Describes an action that should be exposed in the in-game controls menu.
    /// </summary>
    [Serializable]
    public class BetterInputRemappableAction
    {
        public BetterInputRemappableAction(string displayName, BetterInputActionReference actionReference)
        {
            this.displayName = displayName;
            this.actionReference = actionReference;
        }

        public string DisplayName => displayName;
        public BetterInputActionReference ActionReference => actionReference;

        [SerializeField] private string displayName;
        [SerializeField] private BetterInputActionReference actionReference;
    }
}
