namespace Template.BetterInputHandling
{
    /// <summary>
    /// Identifies the broad family of input device that should drive glyph selection and UI behavior.
    /// </summary>
    public enum BetterInputDeviceKind
    {
        Unknown = 0,
        KeyboardMouse = 1,
        XboxGamepad = 2,
        PlayStationGamepad = 3,
        GenericGamepad = 4,
        Touch = 5,
        Joystick = 6,
        XR = 7,
    }
}
