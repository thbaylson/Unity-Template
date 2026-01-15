using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PauseMenuManager : MonoBehaviour
{
    [Header("Menu Root (enable/disable)")]
    [SerializeField] private GameObject menuRoot;

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button returnToTitleButton;
    [SerializeField] private Button quitButton;

    [Header("Scene Targets")]
    [SerializeField] private string titleSceneName = "Title";

    [Header("Optional")]
    [SerializeField] private bool pauseOnEscape = true;
    [SerializeField] private bool showCursorWhenPaused = true;

    private bool _isPaused;

    private void Awake()
    {
        if (resumeButton) resumeButton.onClick.AddListener(Resume);
        if (returnToTitleButton) returnToTitleButton.onClick.AddListener(ReturnToTitle);
        if (quitButton) quitButton.onClick.AddListener(Quit);

        SetPaused(false);
    }

    private void Update()
    {
        if (!pauseOnEscape) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SetPaused(!_isPaused);
        }
    }

    public void Resume() => SetPaused(false);

    public void ReturnToTitle()
    {
        SetPaused(false);
        SceneManager.LoadScene(titleSceneName, LoadSceneMode.Single);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void SetPaused(bool paused)
    {
        _isPaused = paused;

        if (menuRoot) menuRoot.SetActive(paused);

        Time.timeScale = paused ? 0f : 1f;

        if (showCursorWhenPaused)
        {
            Cursor.visible = paused;
            Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
        }

        // Make gamepad/keyboard navigation work immediately when the menu opens.
        if (paused && EventSystem.current != null && resumeButton != null)
        {
            EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
        }
    }

    // Unpause if this object is destroyed. Prevents hard locks if the scene changes while paused.
    private void OnDisable()
    {
        if (_isPaused)
        {
            Time.timeScale = 1f;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
