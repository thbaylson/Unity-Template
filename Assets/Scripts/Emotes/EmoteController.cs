using StarterAssets;
using System.Collections.Generic;
using UnityEngine;

namespace Template.Emotes
{
    public class EmoteController : MonoBehaviour
    {
        [Header("Animator")]
        [SerializeField] private int emoteLayerIndex = 1;
        [SerializeField] private string emoteStateName = "Emote";

        [Header("Override")]
        [Tooltip("The clip used by the Emote state in the Animator Controller. This is the key we override.")]
        [SerializeField] private AnimationClip placeholderClip;

        [Header("Slot Bindings")]
        [SerializeField] private EmoteDefinition up;
        [SerializeField] private EmoteDefinition down;
        [SerializeField] private EmoteDefinition left;
        [SerializeField] private EmoteDefinition right;

        private Animator animator;
        private ThirdPersonController thirdPersonController;
        private StarterAssetsInputs inputs;

        private AnimatorOverrideController _overrideController;
        private int _emoteStateHash;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            thirdPersonController = GetComponent<ThirdPersonController>();
            inputs = GetComponent<StarterAssetsInputs>();

            _emoteStateHash = Animator.StringToHash(emoteStateName);

            // Ensure we have an override controller to swap the placeholder clip.
            if (animator.runtimeAnimatorController is AnimatorOverrideController existing)
            {
                _overrideController = existing;
            }
            else
            {
                _overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
                animator.runtimeAnimatorController = _overrideController;
            }
        }

        public void SetBinding(EmoteSlot slot, EmoteDefinition emote)
        {
            switch (slot)
            {
                case EmoteSlot.Up: up = emote; break;
                case EmoteSlot.Down: down = emote; break;
                case EmoteSlot.Left: left = emote; break;
                case EmoteSlot.Right: right = emote; break;
            }
        }

        public EmoteDefinition GetBinding(EmoteSlot slot)
        {
            return slot switch
            {
                EmoteSlot.Up => up,
                EmoteSlot.Down => down,
                EmoteSlot.Left => left,
                EmoteSlot.Right => right,
                _ => null
            };
        }

        public bool TryPlay(EmoteSlot slot)
        {
            var emote = GetBinding(slot);
            if (emote == null || emote.Clip == null) return false;

            if (!CanStartEmote(emote)) return false;

            Play(emote);
            return true;
        }

        private bool CanStartEmote(EmoteDefinition emote)
        {
            if (emote == null || emote.Clip == null)
            {
                return false;
            }

            if (Services.PauseService?.IsPaused ?? false)
            {
                return false;
            }

            if (thirdPersonController != null && !thirdPersonController.Grounded)
            {
                return false;
            }

            if (inputs != null && (inputs.move != Vector2.zero || thirdPersonController.CurrentSpeed != 0f))
            {
                return false;
            }

            return true;
        }

        private void Play(EmoteDefinition emote)
        {
            // Override the one clip used by the Emote state.
            _overrideController[placeholderClip] = emote.Clip;

            animator.SetLayerWeight(emoteLayerIndex, 1f);
            animator.Play(_emoteStateHash, emoteLayerIndex, 0f);
        }
    }
}