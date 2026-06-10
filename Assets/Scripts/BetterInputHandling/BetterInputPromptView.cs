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

        private BetterInputService subscribedService;

        private void OnEnable()
        {
            TrySubscribe();
            SetVisible(subscribedService != null && subscribedService.HasCurrentPrompt);
        }

        private void OnDisable()
        {
            if (subscribedService != null)
            {
                subscribedService.CurrentPromptChanged -= OnCurrentPromptChanged;
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

        private void TrySubscribe()
        {
            var service = BetterInputService.Instance;
            if (service == null || subscribedService == service)
            {
                return;
            }

            subscribedService = service;
            subscribedService.CurrentPromptChanged += OnCurrentPromptChanged;
            OnCurrentPromptChanged(subscribedService.HasCurrentPrompt, subscribedService.CurrentPrompt);
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
