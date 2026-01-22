using UnityEngine;

namespace Template.Emotes
{
    [CreateAssetMenu(menuName = "Emotes/Emote Definition")]
    public class EmoteDefinition : ScriptableObject
    {
        public string DisplayName;
        public AnimationClip Clip;
    }
}
