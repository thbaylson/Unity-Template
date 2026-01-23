using UnityEngine;

namespace Template.Emotes
{
    [CreateAssetMenu(menuName = "Emotes/Emote Definition")]
    public class EmoteDefinition : ScriptableObject
    {
        public AnimationClip Clip;
        public string DisplayName;
        public bool IsLooping;
    }
}
