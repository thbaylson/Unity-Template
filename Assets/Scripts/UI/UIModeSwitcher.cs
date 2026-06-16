using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.UI;

/// <summary>
/// The class makes it so that using a controller within a UI element auto-hides the cursor. Attempting to use the 
/// cursor will bring it back.
/// </summary>
namespace Template.UI
{
    [MovedFrom(true, null, "Assembly-CSharp", "UIModeSwitcher")]
    public class UIModeSwitcher : MonoBehaviour
    {
        public static UIModeSwitcher Instance { get; private set; }

        [SerializeField] private GameObject pointerBlocker;

        private Graphic pointerBlockerGraphic;
        private InputSystemUIInputModule uiModule;

        private void Awake()
        {
            Instance = this;
            pointerBlockerGraphic = pointerBlocker != null ? pointerBlocker.GetComponent<Graphic>() : null;
            if (pointerBlockerGraphic != null)
            {
                pointerBlockerGraphic.raycastTarget = false;
            }

            if (EventSystem.current != null)
            {
                uiModule = EventSystem.current.GetComponent<InputSystemUIInputModule>();
            }

            ShowPointer();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void OnEnable()
        {
            if (uiModule == null) return;

            if (uiModule.move?.action != null)
            {
                uiModule.move.action.performed += OnNavigate;
            }

            if (uiModule.point?.action != null)
            {
                uiModule.point.action.performed += OnPointer;
            }

            if (uiModule.leftClick?.action != null)
            {
                uiModule.leftClick.action.performed += OnPointer;
            }
        }

        private void OnDisable()
        {
            if (uiModule == null) return;

            if (uiModule.move?.action != null)
            {
                uiModule.move.action.performed -= OnNavigate;
            }

            if (uiModule.point?.action != null)
            {
                uiModule.point.action.performed -= OnPointer;
            }

            if (uiModule.leftClick?.action != null)
            {
                uiModule.leftClick.action.performed -= OnPointer;
            }
        }

        /// <summary>
        /// Restores pointer interaction for menu screens that are opened from keyboard input.
        /// </summary>
        public void ShowPointer()
        {
            BlockAndHideCursor(false);
        }

        private void OnNavigate(InputAction.CallbackContext context)
        {
            if (context.control?.device is Gamepad)
            {
                BlockAndHideCursor(true);
            }
        }

        private void OnPointer(InputAction.CallbackContext context)
        {
            if (context.control?.device is Mouse or Touchscreen or Pen)
            {
                BlockAndHideCursor(false);
            }
        }

        private void BlockAndHideCursor(bool visible)
        {
            if (pointerBlocker != null)
            {
                pointerBlocker.SetActive(visible);
            }

            Cursor.visible = !visible;
        }
    }
}
