using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Template.BetterInputHandling
{
    /// <summary>
    /// Renders a single input glyph from either an action binding or explicit per-device controls.
    /// </summary>
    public class BetterInputGlyphView : MonoBehaviour
    {
        [SerializeField] private Image glyphImage;
        [SerializeField] private TMP_Text fallbackText;
        [SerializeField] private bool useActionReference = true;
        [SerializeField] private BetterInputActionReference actionReference = BetterInputActionReference.Pause;
        [SerializeField] private string keyboardMouseControl = "<Keyboard>/escape";
        [SerializeField] private string gamepadControl = "<Gamepad>/start";

        private BetterInputService subscribedService;

        private void OnEnable()
        {
            TrySubscribe();
            Refresh();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Update()
        {
            if (subscribedService == null)
            {
                TrySubscribe();
                Refresh();
            }
        }

        public void BindAction(BetterInputActionReference reference)
        {
            useActionReference = true;
            actionReference = reference;
            Refresh();
        }

        public void BindExplicitControls(string keyboardMousePath, string gamepadPath)
        {
            useActionReference = false;
            keyboardMouseControl = keyboardMousePath;
            gamepadControl = gamepadPath;
            Refresh();
        }

        public void AssignViews(Image image, TMP_Text text)
        {
            glyphImage = image;
            fallbackText = text;
            Refresh();
        }

        public void Refresh()
        {
            var service = BetterInputService.Instance;
            if (service == null)
            {
                ApplyGlyph(new BetterInputResolvedGlyph(null, string.Empty, string.Empty));
                return;
            }

            var glyph = useActionReference
                ? service.ResolveGlyph(actionReference)
                : service.ResolveControlGlyph(service.ActiveDevice.IsGamepad ? gamepadControl : keyboardMouseControl);

            ApplyGlyph(glyph);
        }

        private void TrySubscribe()
        {
            var service = BetterInputService.Instance;
            if (service == null || subscribedService == service)
            {
                return;
            }

            Unsubscribe();
            subscribedService = service;
            subscribedService.ActiveDeviceChanged += OnActiveDeviceChanged;
            subscribedService.BindingOverridesChanged += OnBindingOverridesChanged;
        }

        private void Unsubscribe()
        {
            if (subscribedService == null)
            {
                return;
            }

            subscribedService.ActiveDeviceChanged -= OnActiveDeviceChanged;
            subscribedService.BindingOverridesChanged -= OnBindingOverridesChanged;
            subscribedService = null;
        }

        private void OnActiveDeviceChanged(BetterInputDeviceProfile _)
        {
            Refresh();
        }

        private void OnBindingOverridesChanged()
        {
            Refresh();
        }

        private void ApplyGlyph(BetterInputResolvedGlyph glyph)
        {
            if (glyphImage != null)
            {
                glyphImage.sprite = glyph.Sprite;
                glyphImage.enabled = glyph.HasSprite;
                glyphImage.preserveAspect = true;
            }

            if (fallbackText != null)
            {
                fallbackText.text = glyph.HasSprite ? string.Empty : glyph.TextFallback;
                fallbackText.enabled = !glyph.HasSprite;
            }
        }
    }
}
