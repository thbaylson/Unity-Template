using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Template.UI;

namespace Template.BetterInputHandling
{
    /// <summary>
    /// Template integration component that creates BetterInputHandling HUD widgets under the existing UIService HUD layer.
    /// </summary>
    public class BetterInputHudInstaller : MonoBehaviour
    {
        private bool installed;

        private void Start()
        {
            TryInstall();
        }

        private void Update()
        {
            if (!installed)
            {
                TryInstall();
            }
        }

        private void TryInstall()
        {
            if (installed || UIService.Instance == null)
            {
                return;
            }

            var hudLayer = UIService.Instance.GetLayer(UiLayer.Hud) as RectTransform;
            if (hudLayer == null)
            {
                return;
            }

            CreatePausePrompt(hudLayer);
            CreateContextPrompt(hudLayer);
            CreateDebugText(hudLayer);
            installed = true;
        }

        private static void CreatePausePrompt(RectTransform parent)
        {
            var root = CreatePanel("BetterInputPausePrompt", parent, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(24f, 24f), new Vector2(156f, 38f));
            var layout = root.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.padding = new RectOffset(10, 12, 6, 6);
            layout.spacing = 8f;
            layout.childControlWidth = false;
            layout.childControlHeight = true;

            var glyph = CreateGlyph("PauseGlyph", root, new Vector2(28f, 28f));
            glyph.BindAction(BetterInputActionReference.Pause);
            var label = CreateText("PauseLabel", root, "Pause", 18f, TextAlignmentOptions.MidlineLeft);
            label.rectTransform.sizeDelta = new Vector2(82f, 28f);
        }

        private static void CreateContextPrompt(RectTransform parent)
        {
            var root = CreatePanel("BetterInputContextPrompt", parent, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 92f), new Vector2(220f, 44f));
            var layout = root.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.padding = new RectOffset(12, 14, 7, 7);
            layout.spacing = 10f;
            layout.childControlWidth = false;
            layout.childControlHeight = true;

            var glyph = CreateGlyph("ContextGlyph", root, new Vector2(30f, 30f));
            var label = CreateText("ContextLabel", root, string.Empty, 20f, TextAlignmentOptions.MidlineLeft);
            label.rectTransform.sizeDelta = new Vector2(150f, 30f);

            var view = root.gameObject.AddComponent<BetterInputPromptView>();
            view.AssignViews(glyph, label);
        }

        private static void CreateDebugText(RectTransform parent)
        {
            var root = CreateRect("BetterInputDeviceDebug", parent, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -18f), new Vector2(280f, 28f));
            var text = CreateText("DebugText", root, "Input: Unknown Device", 15f, TextAlignmentOptions.MidlineLeft);
            text.color = new Color(1f, 1f, 1f, 0.72f);
            text.rectTransform.anchorMin = Vector2.zero;
            text.rectTransform.anchorMax = Vector2.one;
            text.rectTransform.offsetMin = Vector2.zero;
            text.rectTransform.offsetMax = Vector2.zero;

            var debugText = root.gameObject.AddComponent<BetterInputActiveDeviceDebugText>();
            debugText.AssignLabel(text);
        }

        private static BetterInputGlyphView CreateGlyph(string name, RectTransform parent, Vector2 size)
        {
            var glyphRoot = CreateRect(name, parent, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f), Vector2.zero, size);
            var imageObject = CreateRect("Image", glyphRoot, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            var image = imageObject.gameObject.AddComponent<Image>();
            image.raycastTarget = false;

            var fallback = CreateText("Fallback", glyphRoot, string.Empty, 16f, TextAlignmentOptions.Center);
            fallback.rectTransform.anchorMin = Vector2.zero;
            fallback.rectTransform.anchorMax = Vector2.one;
            fallback.rectTransform.offsetMin = Vector2.zero;
            fallback.rectTransform.offsetMax = Vector2.zero;

            var view = glyphRoot.gameObject.AddComponent<BetterInputGlyphView>();
            view.AssignViews(image, fallback);
            return view;
        }

        private static TextMeshProUGUI CreateText(string name, RectTransform parent, string text, float fontSize, TextAlignmentOptions alignment)
        {
            var rect = CreateRect(name, parent, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(100f, 28f));
            var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.color = Color.white;
            label.raycastTarget = false;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            return label;
        }

        private static RectTransform CreatePanel(string name, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 size)
        {
            var rect = CreateRect(name, parent, anchorMin, anchorMax, pivot, anchoredPosition, size);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.58f);
            image.raycastTarget = false;
            return rect;
        }

        private static RectTransform CreateRect(string name, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 size)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            return rect;
        }
    }
}
