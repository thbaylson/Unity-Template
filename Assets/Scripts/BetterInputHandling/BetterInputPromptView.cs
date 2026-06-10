using TMPro;
using UnityEngine;

namespace Template.BetterInputHandling
{
    /// <summary>
    /// Renders the currently active context-aware prompt from BetterInputService.
    /// </summary>
    public class BetterInputPromptView : MonoBehaviour
    {
        [SerializeField] private BetterInputGlyphView glyphView;
        [SerializeField] private TMP_Text promptText;
        [SerializeField] private CanvasGroup canvasGroup;

        private void OnEnable()
        {
            if (BetterInputService.Instance != null)
            {
                BetterInputService.Instance.CurrentPromptChanged += OnCurrentPromptChanged;
                OnCurrentPromptChanged(BetterInputService.Instance.HasCurrentPrompt, BetterInputService.Instance.CurrentPrompt);
            }
            else
            {
                SetVisible(false);
            }
        }

        private void OnDisable()
        {
            if (BetterInputService.Instance != null)
            {
                BetterInputService.Instance.CurrentPromptChanged -= OnCurrentPromptChanged;
            }
        }

        public void AssignViews(BetterInputGlyphView glyph, TMP_Text label)
        {
            glyphView = glyph;
            promptText = label;
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        private void OnCurrentPromptChanged(bool hasPrompt, BetterInputPrompt prompt)
        {
            SetVisible(hasPrompt);
            if (!hasPrompt)
            {
                return;
            }

            glyphView.BindAction(prompt.ActionReference);
            if (promptText != null)
            {
                promptText.text = prompt.PromptText;
            }
        }

        private void SetVisible(bool visible)
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            if (canvasGroup == null)
            {
                return;
            }

            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = false;
        }
    }
}
