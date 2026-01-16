using UnityEngine;
using UnityEngine.InputSystem;

public interface IPauseService
{
    bool IsPaused { get; }

    void RegisterPlayerInput(PlayerInput playerInput);
    void RegisterMenu(PauseMenuUI menu);

    void Toggle();
    void SetPaused(bool paused);
}

public class PauseService : MonoBehaviour, IPauseService
{
    public bool IsPaused { get; private set; } = false;

    [SerializeField] private string gameplayMap = "Player";
    [SerializeField] private string uiMap = "UI";

    private PlayerInput playerInput;
    private PauseMenuUI menuUI;

    private void Awake()
    {
        // TODO: Need to implement Singleton class inheritance.
        Services.PauseService = this;
        DontDestroyOnLoad(gameObject);
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
    }

    private void ApplyInputMap()
    {
        if (playerInput == null) return;
        playerInput.SwitchCurrentActionMap(IsPaused ? uiMap : gameplayMap);
    }

    // This should never be destroyed, but just in case, unpause.
    private void OnDisable()
    {
        if (IsPaused) SetPaused(false);
    }
}
