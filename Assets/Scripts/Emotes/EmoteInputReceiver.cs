using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Template.Emotes
{
    [RequireComponent(typeof(EmoteController))]
    public class EmoteInputReceiver : MonoBehaviour
    {
        [SerializeField] private EmoteController emoteController;

#if ENABLE_INPUT_SYSTEM
        public void OnEmoteUp(InputValue value)
        {
            if (value == null || !value.isPressed) return;
            emoteController.TryPlay(EmoteSlot.Up);
        }

        public void OnEmoteDown(InputValue value)
        {
            if (value == null || !value.isPressed) return;
            emoteController.TryPlay(EmoteSlot.Down);
        }

        public void OnEmoteLeft(InputValue value)
        {
            if (value == null || !value.isPressed) return;
            emoteController.TryPlay(EmoteSlot.Left);
        }

        public void OnEmoteRight(InputValue value)
        {
            if (value == null || !value.isPressed) return;
            emoteController.TryPlay(EmoteSlot.Right);
        }
#endif
    }
}
