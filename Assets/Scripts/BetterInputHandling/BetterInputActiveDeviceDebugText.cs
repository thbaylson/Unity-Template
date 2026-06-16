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

        private BetterInputService subscribedService;

        private void OnEnable()
        {
            TrySubscribe();
        }

        private void OnDisable()
        {
            if (subscribedService != null)
            {
                subscribedService.ActiveDeviceChanged -= OnActiveDeviceChanged;
                subscribedService = null;
            }
        }

        private void Update()
        {
            if (subscribedService == null)
            {
                TrySubscribe();
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

        private void TrySubscribe()
        {
            var service = BetterInputService.Instance;
            if (service == null || subscribedService == service)
            {
                return;
            }

            subscribedService = service;
            subscribedService.ActiveDeviceChanged += OnActiveDeviceChanged;
            OnActiveDeviceChanged(subscribedService.ActiveDevice);
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
