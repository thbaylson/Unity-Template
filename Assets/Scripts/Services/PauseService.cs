using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public interface IPauseService
{
    bool IsPaused { get; }
    event Action<bool> PausedChanged;

    void RegisterPlayerInput(PlayerInput playerInput);
    void RegisterMenu(PauseMenuUI menu);

    void Toggle();
    void SetPaused(bool paused);
}

public class PauseService : MonoBehaviour, IPauseService
{
    public bool IsPaused { get; private set; } = false;
    public event Action<bool> PausedChanged;

    [SerializeField] private string gameplayMap = "Player";
    [SerializeField] private string uiMap = "UI";

    private PlayerInput playerInput;
    private PauseMenuUI menuUI;

    private InputAction _cancelAction;

    private void Awake()
    {
        // TODO: Need to implement Singleton class inheritance.
        Services.PauseService = this;
    }

    private void OnEnable()
    {
        var uiModule = EventSystem.current?.GetComponent<InputSystemUIInputModule>();
        if (uiModule != null && uiModule.cancel != null)
        {
            _cancelAction = uiModule.cancel.action;
            _cancelAction.performed += OnCancelPerformed;
        }
    }

    public void RegisterPlayerInput(PlayerInput playerInput)
    {
        this.playerInput = playerInput;
        ApplyInputMap();
    }

    public void RegisterMenu(PauseMenuUI menu)
    {
        menuUI = menu;
        menuUI.SetVisible(IsPaused);
    }

    public void Toggle() => SetPaused(!IsPaused);

    public void SetPaused(bool paused)
    {
        if (IsPaused == paused) return;

        IsPaused = paused;

        Time.timeScale = paused ? 0f : 1f;
        Cursor.visible = paused;
        Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;

        ApplyInputMap();
        menuUI?.SetVisible(paused);

        PausedChanged?.Invoke(paused);
    }

    private void OnCancelPerformed(InputAction.CallbackContext ctx)
    {
        SetPaused(false);
    }

    private void ApplyInputMap()
    {
        if (playerInput == null) return;
        playerInput.SwitchCurrentActionMap(IsPaused ? uiMap : gameplayMap);
    }

    // This should never be destroyed, but just in case, unpause.
    private void OnDisable()
    {
        if (_cancelAction != null)
        {
            _cancelAction.performed -= OnCancelPerformed;
            _cancelAction = null;
        }

        if (IsPaused) SetPaused(false);
    }
}
