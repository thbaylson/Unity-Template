using UnityEngine;

namespace Template.BetterInputHandling
{
    /// <summary>
    /// Runtime prompt shown to the player for the currently available contextual action.
    /// </summary>
    public readonly struct BetterInputPrompt
    {
        public BetterInputPrompt(
            BetterInputActionReference actionReference,
            string promptText,
            int priority,
            Object owner,
            IBetterInputPromptProvider provider)
        {
            ActionReference = actionReference;
            PromptText = promptText;
            Priority = priority;
            Owner = owner;
            Provider = provider;
        }

        public BetterInputActionReference ActionReference { get; }
        public string PromptText { get; }
        public int Priority { get; }
        public Object Owner { get; }
        public IBetterInputPromptProvider Provider { get; }
    }

    /// <summary>
    /// Query context passed to prompt providers when the detector evaluates nearby prompt candidates.
    /// </summary>
    public readonly struct BetterInputPromptQuery
    {
        public BetterInputPromptQuery(Transform requester, Vector3 requesterPosition, float distance)
        {
            Requester = requester;
            RequesterPosition = requesterPosition;
            Distance = distance;
        }

        public Transform Requester { get; }
        public Vector3 RequesterPosition { get; }
        public float Distance { get; }
    }

    /// <summary>
    /// Implemented by objects that can publish context-aware input prompts.
    /// </summary>
    public interface IBetterInputPromptProvider
    {
        bool TryGetPrompt(BetterInputPromptQuery query, out BetterInputPrompt prompt);
        void ExecutePrompt(BetterInputPrompt prompt);
    }
}
