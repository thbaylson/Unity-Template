using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Template.BetterInputHandling
{
    /// <summary>
    /// Central runtime service for active device detection, glyph resolution, action map switching, prompts, and rebinding.
    /// </summary>
    public class BetterInputService : MonoBehaviour
    {
        [SerializeField] private BetterInputSettings settings;
        [SerializeField, Min(0.01f)] private float stickActivityThreshold = 0.2f;
        [SerializeField, Min(0.01f)] private float triggerActivityThreshold = 0.2f;
        [SerializeField, Min(0.01f)] private float mouseDeltaThreshold = 0.01f;

        private PlayerInput playerInput;
        private InputActionRebindingExtensions.RebindingOperation rebindOperation;
        private bool hasCurrentPrompt;
        private BetterInputPrompt currentPrompt;

        public static BetterInputService Instance { get; private set; }
        public BetterInputSettings Settings => settings;
        public BetterInputDeviceProfile ActiveDevice { get; private set; } = BetterInputDeviceProfile.Unknown;
        public bool HasCurrentPrompt => hasCurrentPrompt;
        public BetterInputPrompt CurrentPrompt => currentPrompt;

        public event Action<BetterInputDeviceProfile> ActiveDeviceChanged;
        public event Action BindingOverridesChanged;
        public event Action<BetterInputActionReference> ActionPerformed;
        public event Action<bool, BetterInputPrompt> CurrentPromptChanged;

        private InputActionAsset ActiveActions => playerInput != null && playerInput.actions != null
            ? playerInput.actions
            : settings != null
                ? settings.InputActions
                : null;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()
        {
            InputSystem.onDeviceChange += OnDeviceChange;
            ApplySavedBindingOverrides(ActiveActions);
        }

        private void OnDisable()
        {
            InputSystem.onDeviceChange -= OnDeviceChange;
            UnregisterPlayerInput();
            rebindOperation?.Dispose();
            rebindOperation = null;
        }

        private void Update()
        {
            DetectPolledInputActivity();
        }

        public void RegisterPlayerInput(PlayerInput input)
        {
            if (playerInput == input)
            {
                return;
            }

            UnregisterPlayerInput();
            playerInput = input;

            if (playerInput == null)
            {
                return;
            }

            ApplySavedBindingOverrides(playerInput.actions);
            playerInput.onControlsChanged += OnPlayerControlsChanged;
            playerInput.onActionTriggered += OnActionTriggered;
            SetActiveDevice(BetterInputDeviceProfile.FromPlayerInput(playerInput));
        }

        public void SwitchCurrentActionMap(string actionMapName)
        {
            if (playerInput == null || string.IsNullOrWhiteSpace(actionMapName))
            {
                return;
            }

            if (playerInput.currentActionMap != null && playerInput.currentActionMap.name == actionMapName)
            {
                return;
            }

            playerInput.SwitchCurrentActionMap(actionMapName);
        }

        public void NotifyInputDeviceUsed(InputDevice device)
        {
            if (device != null)
            {
                SetActiveDevice(BetterInputDeviceProfile.FromInputDevice(device));
            }
        }

        public InputAction FindAction(BetterInputActionReference actionReference)
        {
            if (!actionReference.IsValid)
            {
                return null;
            }

            var actions = ActiveActions;
            return actions?.FindAction(actionReference.ToString(), false);
        }

        public BetterInputResolvedGlyph ResolveGlyph(BetterInputActionReference actionReference)
        {
            var action = FindAction(actionReference);
            if (action == null)
            {
                return ResolveControlGlyph(string.Empty);
            }

            var bindingIndex = FindBestBindingIndex(action);
            if (bindingIndex >= 0)
            {
                return ResolveControlGlyph(action.bindings[bindingIndex].effectivePath);
            }

            return action.bindings.Count > 0
                ? ResolveControlGlyph(action.bindings[0].effectivePath)
                : ResolveControlGlyph(string.Empty);
        }

        public BetterInputResolvedGlyph ResolveControlGlyph(string controlPathOrKey)
        {
            var controlKey = BetterInputControlPathUtility.NormalizeControlPath(controlPathOrKey);
            var fallbackText = BetterInputControlPathUtility.ToDisplayName(controlKey);
            var glyphSet = settings != null ? settings.GetGlyphSet(ActiveDevice.Kind) : null;

            if (glyphSet != null && glyphSet.TryGetGlyph(controlKey, out var glyph))
            {
                return glyph;
            }

            return new BetterInputResolvedGlyph(null, fallbackText, controlKey);
        }

        public string GetBindingDisplayName(BetterInputActionReference actionReference)
        {
            var action = FindAction(actionReference);
            if (action == null)
            {
                return string.Empty;
            }

            var bindingIndex = FindBestBindingIndex(action);
            if (bindingIndex < 0)
            {
                return string.Empty;
            }

            return action.GetBindingDisplayString(bindingIndex);
        }

        public void SetCurrentPrompt(BetterInputPrompt prompt)
        {
            hasCurrentPrompt = true;
            currentPrompt = prompt;
            CurrentPromptChanged?.Invoke(true, currentPrompt);
        }

        public void ClearCurrentPrompt()
        {
            if (!hasCurrentPrompt)
            {
                return;
            }

            hasCurrentPrompt = false;
            currentPrompt = default;
            CurrentPromptChanged?.Invoke(false, currentPrompt);
        }

        public void StartInteractiveRebind(
            BetterInputActionReference actionReference,
            Action<string> statusChanged,
            Action<bool> completed)
        {
            if (rebindOperation != null)
            {
                statusChanged?.Invoke("Already rebinding.");
                completed?.Invoke(false);
                return;
            }

            var action = FindAction(actionReference);
            if (action == null)
            {
                statusChanged?.Invoke("Action unavailable.");
                completed?.Invoke(false);
                return;
            }

            var bindingIndex = FindBestBindingIndex(action);
            if (bindingIndex < 0)
            {
                statusChanged?.Invoke("No binding to replace.");
                completed?.Invoke(false);
                return;
            }

            statusChanged?.Invoke("Press a new input...");
            var wasEnabled = action.enabled;
            action.Disable();

            rebindOperation = action.PerformInteractiveRebinding(bindingIndex)
                .WithCancelingThrough("<Keyboard>/escape")
                .OnCancel(operation =>
                {
                    operation.Dispose();
                    rebindOperation = null;
                    if (wasEnabled)
                    {
                        action.Enable();
                    }

                    statusChanged?.Invoke("Rebind cancelled.");
                    completed?.Invoke(false);
                })
                .OnComplete(operation =>
                {
                    operation.Dispose();
                    rebindOperation = null;
                    if (wasEnabled)
                    {
                        action.Enable();
                    }

                    SaveBindingOverrides();
                    statusChanged?.Invoke(GetBindingDisplayName(actionReference));
                    completed?.Invoke(true);
                });

            rebindOperation.Start();
        }

        public void ResetBinding(BetterInputActionReference actionReference)
        {
            var action = FindAction(actionReference);
            if (action == null)
            {
                return;
            }

            var bindingIndex = FindBestBindingIndex(action);
            if (bindingIndex >= 0)
            {
                action.RemoveBindingOverride(bindingIndex);
                SaveBindingOverrides();
            }
        }

        public void ResetAllBindings()
        {
            var actions = ActiveActions;
            if (actions == null)
            {
                return;
            }

            actions.RemoveAllBindingOverrides();
            SaveBindingOverrides();
        }

        private void OnPlayerControlsChanged(PlayerInput input)
        {
            SetActiveDevice(BetterInputDeviceProfile.FromPlayerInput(input));
        }

        private void OnActionTriggered(InputAction.CallbackContext context)
        {
            if (context.phase != InputActionPhase.Performed)
            {
                return;
            }

            if (context.control != null)
            {
                SetActiveDevice(BetterInputDeviceProfile.FromInputDevice(context.control.device));
            }

            var mapName = context.action?.actionMap?.name;
            var actionName = context.action?.name;
            if (!string.IsNullOrWhiteSpace(mapName) && !string.IsNullOrWhiteSpace(actionName))
            {
                ActionPerformed?.Invoke(new BetterInputActionReference(mapName, actionName));
            }
        }

        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (change == InputDeviceChange.Removed && playerInput != null)
            {
                SetActiveDevice(BetterInputDeviceProfile.FromPlayerInput(playerInput));
            }
        }

        private void DetectPolledInputActivity()
        {
            foreach (var gamepad in Gamepad.all)
            {
                if (WasGamepadUsed(gamepad))
                {
                    SetActiveDevice(BetterInputDeviceProfile.FromInputDevice(gamepad));
                    return;
                }
            }

            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
            {
                SetActiveDevice(BetterInputDeviceProfile.FromInputDevice(Keyboard.current));
                return;
            }

            if (Mouse.current != null && WasMouseUsed(Mouse.current))
            {
                SetActiveDevice(BetterInputDeviceProfile.FromInputDevice(Mouse.current));
            }
        }

        private bool WasGamepadUsed(Gamepad gamepad)
        {
            if (gamepad == null)
            {
                return false;
            }

            var stickThresholdSquared = stickActivityThreshold * stickActivityThreshold;
            return gamepad.buttonSouth.wasPressedThisFrame
                   || gamepad.buttonEast.wasPressedThisFrame
                   || gamepad.buttonWest.wasPressedThisFrame
                   || gamepad.buttonNorth.wasPressedThisFrame
                   || gamepad.startButton.wasPressedThisFrame
                   || gamepad.selectButton.wasPressedThisFrame
                   || gamepad.leftShoulder.wasPressedThisFrame
                   || gamepad.rightShoulder.wasPressedThisFrame
                   || gamepad.leftStickButton.wasPressedThisFrame
                   || gamepad.rightStickButton.wasPressedThisFrame
                   || gamepad.dpad.up.wasPressedThisFrame
                   || gamepad.dpad.down.wasPressedThisFrame
                   || gamepad.dpad.left.wasPressedThisFrame
                   || gamepad.dpad.right.wasPressedThisFrame
                   || gamepad.leftTrigger.ReadValue() > triggerActivityThreshold
                   || gamepad.rightTrigger.ReadValue() > triggerActivityThreshold
                   || gamepad.leftStick.ReadValue().sqrMagnitude > stickThresholdSquared
                   || gamepad.rightStick.ReadValue().sqrMagnitude > stickThresholdSquared;
        }

        private bool WasMouseUsed(Mouse mouse)
        {
            if (mouse == null)
            {
                return false;
            }

            var mouseThresholdSquared = mouseDeltaThreshold * mouseDeltaThreshold;
            return mouse.leftButton.wasPressedThisFrame
                   || mouse.rightButton.wasPressedThisFrame
                   || mouse.middleButton.wasPressedThisFrame
                   || mouse.forwardButton.wasPressedThisFrame
                   || mouse.backButton.wasPressedThisFrame
                   || mouse.scroll.ReadValue().sqrMagnitude > mouseThresholdSquared
                   || mouse.delta.ReadValue().sqrMagnitude > mouseThresholdSquared;
        }

        private void SetActiveDevice(BetterInputDeviceProfile deviceProfile)
        {
            if (deviceProfile.Kind == ActiveDevice.Kind && deviceProfile.DisplayName == ActiveDevice.DisplayName)
            {
                return;
            }

            ActiveDevice = deviceProfile;
            ActiveDeviceChanged?.Invoke(ActiveDevice);
            CurrentPromptChanged?.Invoke(hasCurrentPrompt, currentPrompt);
        }

        private int FindBestBindingIndex(InputAction action)
        {
            if (action == null)
            {
                return -1;
            }

            var groups = GetActiveBindingGroups();
            foreach (var group in groups)
            {
                for (var i = 0; i < action.bindings.Count; i++)
                {
                    var binding = action.bindings[i];
                    if (binding.isComposite || binding.isPartOfComposite)
                    {
                        continue;
                    }

                    if (BetterInputControlPathUtility.BindingMatchesAnyGroup(binding.groups, new[] { group }))
                    {
                        return i;
                    }
                }
            }

            return -1;
        }

        private IReadOnlyList<string> GetActiveBindingGroups()
        {
            var groups = new List<string>();
            if (!string.IsNullOrWhiteSpace(ActiveDevice.PrimaryBindingGroup))
            {
                groups.Add(ActiveDevice.PrimaryBindingGroup);
            }

            foreach (var alias in ActiveDevice.BindingGroupAliases)
            {
                if (!string.IsNullOrWhiteSpace(alias) && !groups.Contains(alias))
                {
                    groups.Add(alias);
                }
            }

            if (groups.Count == 0)
            {
                groups.Add("KeyboardMouse");
                groups.Add("Keyboard&Mouse");
            }

            return groups;
        }

        private void ApplySavedBindingOverrides(InputActionAsset actions)
        {
            if (actions == null || settings == null)
            {
                return;
            }

            var json = PlayerPrefs.GetString(settings.BindingOverridesPlayerPrefsKey, string.Empty);
            if (!string.IsNullOrWhiteSpace(json))
            {
                actions.LoadBindingOverridesFromJson(json);
                BindingOverridesChanged?.Invoke();
            }
        }

        private void SaveBindingOverrides()
        {
            var actions = ActiveActions;
            if (actions == null || settings == null)
            {
                return;
            }

            PlayerPrefs.SetString(settings.BindingOverridesPlayerPrefsKey, actions.SaveBindingOverridesAsJson());
            PlayerPrefs.Save();
            BindingOverridesChanged?.Invoke();
            CurrentPromptChanged?.Invoke(hasCurrentPrompt, currentPrompt);
        }

        private void UnregisterPlayerInput()
        {
            if (playerInput == null)
            {
                return;
            }

            playerInput.onControlsChanged -= OnPlayerControlsChanged;
            playerInput.onActionTriggered -= OnActionTriggered;
            playerInput = null;
        }
    }
}
