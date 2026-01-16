using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

/// <summary>
/// The class makes it so that using a controller within a UI element auto-hides the cursor. Attempting to use the 
/// cursor will bring it back.
/// </summary>
public class UIModeSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject pointerBlocker;

    private InputSystemUIInputModule uiModule;

    private void Awake()
    {
        if (EventSystem.current != null)
        {
            uiModule = EventSystem.current.GetComponent<InputSystemUIInputModule>();
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

    private void OnNavigate(InputAction.CallbackContext _)
    {
        BlockAndHideCursor(true);
    }

    private void OnPointer(InputAction.CallbackContext _)
    {
        
        BlockAndHideCursor(false);
    }

    private void BlockAndHideCursor(bool visible)
    {
        pointerBlocker.SetActive(visible);
        Cursor.visible = !visible;
    }
}
