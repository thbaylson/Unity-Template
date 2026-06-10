using TMPro;
using UnityEngine;

namespace Template.BetterInputHandling
{
    /// <summary>
    /// Temporary HUD text that displays the currently active input device for debugging and alignment testing.
    /// </summary>
    public class BetterInputActiveDeviceDebugText : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;

        private void OnEnable()
        {
            if (BetterInputService.Instance != null)
            {
                BetterInputService.Instance.ActiveDeviceChanged += OnActiveDeviceChanged;
                OnActiveDeviceChanged(BetterInputService.Instance.ActiveDevice);
            }
        }

        private void OnDisable()
        {
            if (BetterInputService.Instance != null)
            {
                BetterInputService.Instance.ActiveDeviceChanged -= OnActiveDeviceChanged;
            }
        }

        public void AssignLabel(TMP_Text text)
        {
            label = text;
            if (BetterInputService.Instance != null)
            {
                OnActiveDeviceChanged(BetterInputService.Instance.ActiveDevice);
            }
        }

        private void OnActiveDeviceChanged(BetterInputDeviceProfile profile)
        {
            if (label != null)
            {
                label.text = $"Input: {profile.DisplayName}";
            }
        }
    }
}
