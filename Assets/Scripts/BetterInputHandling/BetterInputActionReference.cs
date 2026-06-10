using System;

namespace Template.BetterInputHandling
{
    /// <summary>
    /// Serializable reference to an input action by map and action name.
    /// </summary>
    [Serializable]
    public struct BetterInputActionReference : IEquatable<BetterInputActionReference>
    {
        public BetterInputActionReference(string actionMapName, string actionName)
        {
            this.actionMapName = actionMapName ?? string.Empty;
            this.actionName = actionName ?? string.Empty;
        }

        public string ActionMapName => actionMapName;
        public string ActionName => actionName;
        public bool IsValid => !string.IsNullOrWhiteSpace(actionMapName) && !string.IsNullOrWhiteSpace(actionName);

        public static BetterInputActionReference Pause => new BetterInputActionReference("Player", "Pause");
        public static BetterInputActionReference Interact => new BetterInputActionReference("Player", "Interact");

        public bool Equals(BetterInputActionReference other)
        {
            return string.Equals(actionMapName, other.actionMapName, StringComparison.Ordinal)
                   && string.Equals(actionName, other.actionName, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is BetterInputActionReference other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((actionMapName != null ? actionMapName.GetHashCode() : 0) * 397)
                       ^ (actionName != null ? actionName.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return IsValid ? $"{actionMapName}/{actionName}" : string.Empty;
        }

        [UnityEngine.SerializeField] private string actionMapName;
        [UnityEngine.SerializeField] private string actionName;
    }
}
