using UnityEngine;

namespace Template.BetterInputHandling
{
    /// <summary>
    /// Simple authoring component for objects that expose one context-aware input prompt.
    /// </summary>
    public class BetterInputPromptSource : MonoBehaviour, IBetterInputPromptProvider
    {
        [SerializeField] private BetterInputActionReference actionReference = BetterInputActionReference.Interact;
        [SerializeField] private string promptText = "Interact";
        [SerializeField] private int priority;
        [SerializeField] private bool hideObjectWhenExecuted;
        [SerializeField] private bool logExecution = true;

        public bool TryGetPrompt(BetterInputPromptQuery query, out BetterInputPrompt prompt)
        {
            prompt = new BetterInputPrompt(actionReference, promptText, priority, this, this);
            return isActiveAndEnabled;
        }

        public void ExecutePrompt(BetterInputPrompt prompt)
        {
            if (logExecution)
            {
                Debug.Log($"Executed input prompt '{promptText}' on {name}.", this);
            }

            if (hideObjectWhenExecuted)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
