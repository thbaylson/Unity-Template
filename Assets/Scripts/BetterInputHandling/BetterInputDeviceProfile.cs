using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace Template.BetterInputHandling
{
    /// <summary>
    /// Describes the currently active input device in normalized terms independent from a specific action asset.
    /// </summary>
    [Serializable]
    public readonly struct BetterInputDeviceProfile
    {
        public BetterInputDeviceProfile(
            BetterInputDeviceKind kind,
            string displayName,
            string primaryBindingGroup,
            IReadOnlyList<string> bindingGroupAliases)
        {
            Kind = kind;
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? "Unknown Device" : displayName;
            PrimaryBindingGroup = primaryBindingGroup ?? string.Empty;
            BindingGroupAliases = bindingGroupAliases ?? Array.Empty<string>();
        }

        public BetterInputDeviceKind Kind { get; }
        public string DisplayName { get; }
        public string PrimaryBindingGroup { get; }
        public IReadOnlyList<string> BindingGroupAliases { get; }
        public bool IsGamepad => Kind == BetterInputDeviceKind.XboxGamepad
                                 || Kind == BetterInputDeviceKind.PlayStationGamepad
                                 || Kind == BetterInputDeviceKind.GenericGamepad;

        public static BetterInputDeviceProfile Unknown { get; } = new BetterInputDeviceProfile(
            BetterInputDeviceKind.Unknown,
            "Unknown Device",
            string.Empty,
            Array.Empty<string>());

        public static BetterInputDeviceProfile FromPlayerInput(PlayerInput playerInput)
        {
            if (playerInput == null)
            {
                return Unknown;
            }

            var device = playerInput.devices.Count > 0 ? playerInput.devices[0] : null;
            return FromControlScheme(playerInput.currentControlScheme, device);
        }

        public static BetterInputDeviceProfile FromControlScheme(string controlScheme, InputDevice device)
        {
            var normalizedScheme = BetterInputControlPathUtility.NormalizeBindingGroup(controlScheme);
            if (normalizedScheme == "keyboardmouse")
            {
                return new BetterInputDeviceProfile(
                    BetterInputDeviceKind.KeyboardMouse,
                    "Keyboard & Mouse",
                    controlScheme,
                    new[] { "KeyboardMouse", "Keyboard&Mouse" });
            }

            if (normalizedScheme == "touch")
            {
                return new BetterInputDeviceProfile(BetterInputDeviceKind.Touch, "Touch", controlScheme, new[] { "Touch" });
            }

            if (normalizedScheme == "joystick")
            {
                return new BetterInputDeviceProfile(BetterInputDeviceKind.Joystick, "Joystick", controlScheme, new[] { "Joystick" });
            }

            if (normalizedScheme == "xr")
            {
                return new BetterInputDeviceProfile(BetterInputDeviceKind.XR, "XR Controller", controlScheme, new[] { "XR" });
            }

            var gamepad = device as Gamepad;
            if (gamepad != null || normalizedScheme.Contains("gamepad") || normalizedScheme.Contains("controller"))
            {
                return FromGamepad(controlScheme, gamepad ?? Gamepad.current);
            }

            if (device is Keyboard || device is Mouse)
            {
                return new BetterInputDeviceProfile(
                    BetterInputDeviceKind.KeyboardMouse,
                    "Keyboard & Mouse",
                    "KeyboardMouse",
                    new[] { "KeyboardMouse", "Keyboard&Mouse" });
            }

            return Unknown;
        }

        public static BetterInputDeviceProfile FromInputDevice(InputDevice device)
        {
            return device switch
            {
                Keyboard => new BetterInputDeviceProfile(
                    BetterInputDeviceKind.KeyboardMouse,
                    "Keyboard & Mouse",
                    "KeyboardMouse",
                    new[] { "KeyboardMouse", "Keyboard&Mouse" }),
                Mouse => new BetterInputDeviceProfile(
                    BetterInputDeviceKind.KeyboardMouse,
                    "Keyboard & Mouse",
                    "KeyboardMouse",
                    new[] { "KeyboardMouse", "Keyboard&Mouse" }),
                Gamepad gamepad => FromGamepad("Gamepad", gamepad),
                _ => Unknown,
            };
        }

        private static BetterInputDeviceProfile FromGamepad(string controlScheme, Gamepad gamepad)
        {
            var layout = gamepad?.layout ?? string.Empty;
            var displayName = gamepad?.displayName ?? "Gamepad";
            var normalizedLayout = layout.ToLowerInvariant();
            var normalizedDisplayName = displayName.ToLowerInvariant();
            var compactLayout = BetterInputControlPathUtility.NormalizeBindingGroup(layout);
            var compactDisplayName = BetterInputControlPathUtility.NormalizeBindingGroup(displayName);
            var compactControlScheme = BetterInputControlPathUtility.NormalizeBindingGroup(controlScheme);

            if (normalizedLayout.Contains("dualshock")
                || normalizedLayout.Contains("dualsense")
                || normalizedLayout.Contains("playstation")
                || normalizedDisplayName.Contains("dualshock")
                || normalizedDisplayName.Contains("dualsense")
                || normalizedDisplayName.Contains("dual sense")
                || normalizedDisplayName.Contains("playstation")
                || normalizedDisplayName.Contains("sony")
                || compactLayout.Contains("dualshock")
                || compactLayout.Contains("dualsense")
                || compactLayout.Contains("playstation")
                || compactDisplayName.Contains("dualshock")
                || compactDisplayName.Contains("dualsense")
                || compactDisplayName.Contains("playstation")
                || compactDisplayName.Contains("sony")
                || compactControlScheme.Contains("ps"))
            {
                return new BetterInputDeviceProfile(
                    BetterInputDeviceKind.PlayStationGamepad,
                    "PlayStation Controller",
                    controlScheme,
                    new[] { "Gamepad", "PS4 Controller", "PlayStation", "PlayStationGamepad" });
            }

            if (normalizedLayout.Contains("xinput")
                || normalizedDisplayName.Contains("xbox")
                || compactControlScheme.Contains("xbox"))
            {
                return new BetterInputDeviceProfile(
                    BetterInputDeviceKind.XboxGamepad,
                    "Xbox Controller",
                    controlScheme,
                    new[] { "Gamepad", "Xbox Controller", "XInput", "XboxGamepad" });
            }

            if (normalizedLayout.Contains("switch")
                || normalizedDisplayName.Contains("switch")
                || normalizedDisplayName.Contains("nintendo")
                || compactLayout.Contains("switch")
                || compactDisplayName.Contains("switch")
                || compactDisplayName.Contains("nintendo")
                || compactControlScheme.Contains("switch"))
            {
                return new BetterInputDeviceProfile(
                    BetterInputDeviceKind.GenericGamepad,
                    displayName,
                    "Switch Controller",
                    new[] { "Switch Controller", "SwitchProControllerHID", "Gamepad" });
            }

            return new BetterInputDeviceProfile(
                BetterInputDeviceKind.GenericGamepad,
                displayName,
                string.IsNullOrWhiteSpace(controlScheme) ? "Gamepad" : controlScheme,
                new[] { "Gamepad" });
        }
    }
}
